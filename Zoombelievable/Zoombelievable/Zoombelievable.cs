using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Zoombelievable {
    public class Zoombelievable : Border {
        #region Private Fields
        private readonly Image _image;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;
        private Point _lastMousePosition;
        private Point _lastImagePoint;
        private bool _isDragging;
        private readonly List<Zoombelievable> _linkedImages = new();
        private bool _isPropagatingEvent;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(Zoombelievable), new PropertyMetadata(null, OnImageSourceChanged));
        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(Zoombelievable), new PropertyMetadata(0.1));
        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(Zoombelievable), new PropertyMetadata(10.0));
        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register(nameof(ZoomFactor), typeof(double), typeof(Zoombelievable), new PropertyMetadata(1.2));
        public static readonly DependencyProperty CurrentZoomProperty =
            DependencyProperty.Register(nameof(CurrentZoom), typeof(double), typeof(Zoombelievable), new PropertyMetadata(1.0, OnCurrentZoomChanged));
        public static readonly DependencyProperty PanOffsetXProperty =
            DependencyProperty.Register(nameof(PanOffsetX), typeof(double), typeof(Zoombelievable), new PropertyMetadata(0.0, OnPanOffsetXChanged));
        public static readonly DependencyProperty PanOffsetYProperty =
            DependencyProperty.Register(nameof(PanOffsetY), typeof(double), typeof(Zoombelievable), new PropertyMetadata(0.0, OnPanOffsetYChanged));
        public static readonly DependencyProperty EnablePanProperty =
            DependencyProperty.Register(nameof(EnablePan), typeof(bool), typeof(Zoombelievable), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableZoomProperty =
            DependencyProperty.Register(nameof(EnableZoom), typeof(bool), typeof(Zoombelievable), new PropertyMetadata(true));
        #endregion

        #region Events
        /// <summary>줌 변경 시 발생</summary>
        public event EventHandler<ZoomChangedEventArgs>? ZoomChanged;
        /// <summary>마우스 이미지 좌표 이동 시 발생</summary>
        public event EventHandler<MousePositionChangedEventArgs>? MousePositionChanged;
        #endregion

        #region Public Properties
        public ImageSource? ImageSource { get => (ImageSource?)GetValue(ImageSourceProperty); set => SetValue(ImageSourceProperty, value); }
        public double MinZoom { get => (double)GetValue(MinZoomProperty); set => SetValue(MinZoomProperty, value); }
        public double MaxZoom { get => (double)GetValue(MaxZoomProperty); set => SetValue(MaxZoomProperty, value); }
        public double ZoomFactor { get => (double)GetValue(ZoomFactorProperty); set => SetValue(ZoomFactorProperty, value); }
        public double CurrentZoom { get => (double)GetValue(CurrentZoomProperty); set => SetValue(CurrentZoomProperty, value); }
        public double PanOffsetX { get => (double)GetValue(PanOffsetXProperty); set => SetValue(PanOffsetXProperty, value); }
        public double PanOffsetY { get => (double)GetValue(PanOffsetYProperty); set => SetValue(PanOffsetYProperty, value); }
        /// <summary>마우스가 가리키는 마지막 이미지 내부 픽셀 좌표</summary>
        public Point LastMouseImagePosition => _lastImagePoint;
        public bool EnablePan { get => (bool)GetValue(EnablePanProperty); set => SetValue(EnablePanProperty, value); }
        public bool EnableZoom { get => (bool)GetValue(EnableZoomProperty); set => SetValue(EnableZoomProperty, value); }
        #endregion

        #region Constructor
        public Zoombelievable() {
            Background = Brushes.Transparent;
            ClipToBounds = true;
            _transformGroup = new TransformGroup();
            _scaleTransform = new ScaleTransform(1, 1);
            _translateTransform = new TranslateTransform(0, 0);
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
            _image = new Image {
                Stretch = Stretch.Uniform,
                RenderTransform = _transformGroup,
                RenderTransformOrigin = new Point(0.5, 0.5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Child = _image;
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            _image.MouseMove += OnImageMouseMove;
        }
        #endregion

        #region Event Handlers
        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (!EnableZoom) return;

            // 현재 마우스 위치
            Point mousePos = e.GetPosition(this);

            // 현재 줌 배율
            double oldZoom = CurrentZoom;

            // 새로운 줌 배율 계산
            double factor = e.Delta > 0 ? ZoomFactor : 1.0 / ZoomFactor;
            double newZoom = Math.Clamp(oldZoom * factor, MinZoom, MaxZoom);

            // 줌 배율이 변경되지 않으면 아무 것도 하지 않음
            if (Math.Abs(newZoom - oldZoom) < 1e-6) return;

            // 새로운 스케일 비율
            double scaleRatio = newZoom / oldZoom;

            // 마우스 위치를 기준으로 이동 변환 조정
            double deltaX = (mousePos.X - this.ActualWidth / 2) * (1 - scaleRatio);
            double deltaY = (mousePos.Y - this.ActualHeight / 2) * (1 - scaleRatio);

            // 새로운 PanOffset 계산
            double newPanX = PanOffsetX * scaleRatio + deltaX;
            double newPanY = PanOffsetY * scaleRatio + deltaY;

            // 줌과 이동 변환 업데이트
            CurrentZoom = newZoom;
            PanOffsetX = newPanX;
            PanOffsetY = newPanY;

            e.Handled = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!EnablePan) return;

            _lastMousePosition = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_isDragging) return;

            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (!_isDragging || !EnablePan) return;

            Point pos = e.GetPosition(this);
            double deltaX = pos.X - _lastMousePosition.X;
            double deltaY = pos.Y - _lastMousePosition.Y;

            // Update DPs, which will trigger callbacks and propagate
            PanOffsetX += deltaX;
            PanOffsetY += deltaY;

            _lastMousePosition = pos;
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e) {
            // 이전 픽셀 좌표 저장 (변경 여부 확인용)
            Point oldImagePoint = _lastImagePoint;

            // --- 초기 유효성 검사 ---
            if (_image.Source == null || _image.Source.Width <= 0 || _image.Source.Height <= 0) {
                _lastImagePoint = new Point(-1, -1);
            } else {
                Point mousePosImageElement = e.GetPosition(_image);

                double imgSrcWidth = _image.Source.Width;
                double imgSrcHeight = _image.Source.Height;
                double imgElemWidth = _image.ActualWidth;
                double imgElemHeight = _image.ActualHeight;

                if (imgElemWidth <= 0 || imgElemHeight <= 0 || double.IsNaN(imgSrcWidth) || double.IsNaN(imgSrcHeight) || imgSrcWidth == 0 || imgSrcHeight == 0) {
                    _lastImagePoint = new Point(-1, -1);
                } else {
                    double scaleX = imgElemWidth / imgSrcWidth;
                    double scaleY = imgElemHeight / imgSrcHeight;
                    double stretchScale = Math.Min(scaleX, scaleY);

                    double renderedWidth = imgSrcWidth * stretchScale;
                    double renderedHeight = imgSrcHeight * stretchScale;

                    double offsetX = (imgElemWidth - renderedWidth) / 2.0;
                    double offsetY = (imgElemHeight - renderedHeight) / 2.0;

                    Point imgPt = new Point(
                        (mousePosImageElement.X - offsetX) / stretchScale,
                        (mousePosImageElement.Y - offsetY) / stretchScale
                    );

                    _lastImagePoint = new Point(
                        Math.Clamp(imgPt.X, 0, imgSrcWidth),
                        Math.Clamp(imgPt.Y, 0, imgSrcHeight)
                    );
                }
            }

            // 이벤트 트리거 및 전파
            if (Math.Abs(_lastImagePoint.X - oldImagePoint.X) > 1e-6 || Math.Abs(_lastImagePoint.Y - oldImagePoint.Y) > 1e-6) {
                MousePositionChanged?.Invoke(this, new MousePositionChangedEventArgs(_lastImagePoint));

                if (!_isPropagatingEvent) {
                    _isPropagatingEvent = true;
                    try {
                        foreach (var linked in _linkedImages.Where(li => li != this)) {
                            if (!linked._isPropagatingEvent) {
                                linked._isPropagatingEvent = true;
                                try {
                                    linked._lastImagePoint = _lastImagePoint;
                                    linked.MousePositionChanged?.Invoke(linked, new MousePositionChangedEventArgs(_lastImagePoint));
                                } finally {
                                    linked._isPropagatingEvent = false;
                                }
                            }
                        }
                    } finally {
                        _isPropagatingEvent = false;
                    }
                }
            }
        }
        #endregion

        #region Static Callbacks
        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Zoombelievable zi && e.NewValue is ImageSource src) {
                // 그냥 이미지 소스만 교체하고, Pan/Zoom 상태는 유지한다.
                zi._image.Source = src;
                // ※ ResetZoomInternal() 호출을 제거하여 Pan/Zoom이 초기화되지 않도록 함.
            }
        }

        private static void OnCurrentZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Zoombelievable zi) {
                double oldZoom = (double)e.OldValue;
                double newZoom = (double)e.NewValue;
                zi._scaleTransform.ScaleX = newZoom;
                zi._scaleTransform.ScaleY = newZoom;

                if (!zi._isPropagatingEvent) {
                    zi._isPropagatingEvent = true;
                    try {
                        foreach (var linked in zi._linkedImages.Where(li => li != zi)) {
                            if (!linked._isPropagatingEvent) {
                                linked._isPropagatingEvent = true;
                                try {
                                    linked.CurrentZoom = newZoom;
                                    linked.ZoomChanged?.Invoke(linked, new ZoomChangedEventArgs((double)linked.GetValue(CurrentZoomProperty), newZoom));
                                } finally {
                                    linked._isPropagatingEvent = false;
                                }
                            }
                        }
                        zi.ZoomChanged?.Invoke(zi, new ZoomChangedEventArgs(oldZoom, newZoom));
                    } finally {
                        zi._isPropagatingEvent = false;
                    }
                } else {
                    zi.ZoomChanged?.Invoke(zi, new ZoomChangedEventArgs(oldZoom, newZoom));
                }
            }
        }

        private static void OnPanOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Zoombelievable zi) {
                double oldPanX = (double)e.OldValue;
                double newPanX = (double)e.NewValue;
                zi._translateTransform.X = newPanX;

                if (!zi._isPropagatingEvent) {
                    zi._isPropagatingEvent = true;
                    try {
                        foreach (var linked in zi._linkedImages.Where(li => li != zi)) {
                            if (!linked._isPropagatingEvent) {
                                linked._isPropagatingEvent = true;
                                try {
                                    linked.PanOffsetX = newPanX;
                                } finally {
                                    linked._isPropagatingEvent = false;
                                }
                            }
                        }
                    } finally {
                        zi._isPropagatingEvent = false;
                    }
                }
            }
        }

        private static void OnPanOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Zoombelievable zi) {
                double oldPanY = (double)e.OldValue;
                double newPanY = (double)e.NewValue;
                zi._translateTransform.Y = newPanY;

                if (!zi._isPropagatingEvent) {
                    zi._isPropagatingEvent = true;
                    try {
                        foreach (var linked in zi._linkedImages.Where(li => li != zi)) {
                            if (!linked._isPropagatingEvent) {
                                linked._isPropagatingEvent = true;
                                try {
                                    linked.PanOffsetY = newPanY;
                                } finally {
                                    linked._isPropagatingEvent = false;
                                }
                            }
                        }
                    } finally {
                        zi._isPropagatingEvent = false;
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void ResetZoomInternal() {
            Point oldImagePoint = _lastImagePoint;

            CurrentZoom = 1.0;
            PanOffsetX = 0;
            PanOffsetY = 0;
            _lastImagePoint = new Point(0, 0);

            if (!_isPropagatingEvent) {
                _isPropagatingEvent = true;
                try {
                    MousePositionChanged?.Invoke(this, new MousePositionChangedEventArgs(_lastImagePoint));
                    foreach (var linked in _linkedImages.Where(li => li != this)) {
                        if (!linked._isPropagatingEvent) {
                            linked._isPropagatingEvent = true;
                            try {
                                linked._lastImagePoint = _lastImagePoint;
                                linked.MousePositionChanged?.Invoke(linked, new MousePositionChangedEventArgs(_lastImagePoint));
                            } finally {
                                linked._isPropagatingEvent = false;
                            }
                        }
                    }
                } finally {
                    _isPropagatingEvent = false;
                }
            } else {
                MousePositionChanged?.Invoke(this, new MousePositionChangedEventArgs(_lastImagePoint));
            }
        }
        #endregion

        #region Public Methods
        public void LinkWith(Zoombelievable other) {
            if (other == null || other == this || _linkedImages.Contains(other)) return;

            _linkedImages.Add(other);
            other._linkedImages.Add(this);

            if (!this._isPropagatingEvent) {
                this._isPropagatingEvent = true;
                try {
                    if (!other._isPropagatingEvent) {
                        other._isPropagatingEvent = true;
                        try {
                            other.CurrentZoom = CurrentZoom;
                            other.PanOffsetX = PanOffsetX;
                            other.PanOffsetY = PanOffsetY;

                            other._lastImagePoint = _lastImagePoint;
                            other.MousePositionChanged?.Invoke(other, new MousePositionChangedEventArgs(_lastImagePoint));
                        } finally {
                            other._isPropagatingEvent = false;
                        }
                    }
                } finally {
                    this._isPropagatingEvent = false;
                }
            }
        }

        public void UnlinkFrom(Zoombelievable other) {
            if (other == null) return;
            _linkedImages.Remove(other);
            other._linkedImages.Remove(this);
        }

        public void UnlinkAll() {
            foreach (var other in _linkedImages.ToArray()) other._linkedImages.Remove(this);
            _linkedImages.Clear();
        }

        /// <summary>
        /// 줌과 패닝을 초기 상태(Zoom = 1.0, Pan = 0,0)로 리셋하고 이벤트를 발생시킵니다.
        /// </summary>
        public void ResetZoom() {
            ResetZoomInternal();
        }

        public void ZoomIn(double factor = 0) {
            if (!EnableZoom) return;
            double old = CurrentZoom;
            double f = factor <= 0 ? ZoomFactor : factor;
            double newZoom = Math.Min(old * f, MaxZoom);

            CurrentZoom = newZoom;
        }

        public void CenterOn(Point imagePoint) {
            double imgSrcWidth = _image.Source?.Width ?? 0;
            double imgSrcHeight = _image.Source?.Height ?? 0;
            double imgElemWidth = _image.ActualWidth;
            double imgElemHeight = _image.ActualHeight;

            if (imgElemWidth <= 0 || imgElemHeight <= 0 || imgSrcWidth <= 0 || imgSrcHeight <= 0 || double.IsNaN(imgSrcWidth) || double.IsNaN(imgSrcHeight)) {
                return;
            }

            double scaleX = imgElemWidth / imgSrcWidth;
            double scaleY = imgElemHeight / imgSrcHeight;
            double stretchScale = Math.Min(scaleX, scaleY);

            double renderedWidth = imgSrcWidth * stretchScale;
            double renderedHeight = imgSrcHeight * stretchScale;

            double offsetX = (imgElemWidth - renderedWidth) / 2.0;
            double offsetY = (imgElemHeight - renderedHeight) / 2.0;

            double elementX = offsetX + imagePoint.X * stretchScale;
            double elementY = offsetY + imagePoint.Y * stretchScale;

            double newPanX = -(elementX - imgElemWidth / 2.0) * CurrentZoom;
            double newPanY = -(elementY - imgElemHeight / 2.0) * CurrentZoom;

            PanOffsetX = newPanX;
            PanOffsetY = newPanY;
        }

        public void ZoomOut(double factor = 0) {
            if (!EnableZoom) return;
            double old = CurrentZoom;
            double f = factor <= 0 ? ZoomFactor : factor;
            double newZoom = Math.Max(old / f, MinZoom);

            CurrentZoom = newZoom;
        }
        #endregion
    }

    /// <summary>
    /// 줌 변경 이벤트 인자
    /// </summary>
    public class ZoomChangedEventArgs : EventArgs {
        public double OldZoom { get; }
        public double NewZoom { get; }
        public ZoomChangedEventArgs(double oldZoom, double newZoom) {
            OldZoom = oldZoom; NewZoom = newZoom;
        }
    }

    /// <summary>
    /// 마우스 이미지 좌표 변경 이벤트 인자
    /// </summary>
    public class MousePositionChangedEventArgs : EventArgs {
        public Point ImagePoint { get; }
        public MousePositionChangedEventArgs(Point pt) {
            ImagePoint = pt;
        }
    }
}