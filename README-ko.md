# Zoombelievable

Zoombelievable은 이미지에 대한 대화형 확대/축소 및 이동(패닝) 기능을 제공하는 사용자 정의 WPF 컨트롤입니다. WPF 애플리케이션에 쉽게 사용하고 통합할 수 있도록 설계되었으며, 상세 이미지를 보기 위한 부드러운 사용자 경험을 제공합니다. 또한 여러 인스턴스를 함께 연결하여 변환을 동기화하는 기능을 지원합니다.

## 주요 기능

* **마우스 휠 확대/축소**: 마우스 휠을 사용하여 이미지를 확대하거나 축소합니다. 확대/축소는 마우스 커서 위치를 중심으로 이루어집니다.
* **마우스 드래그 이동(패닝)**: 마우스 왼쪽 버튼을 클릭하고 드래그하여 이미지를 이동합니다.
* **확대/축소 제한 설정**: 최소 및 최대 확대/축소 레벨을 설정할 수 있습니다 (`MinZoom`, `MaxZoom`).
* **확대/축소 비율 조절**: 마우스 휠 스크롤에 따른 확대/축소 비율을 제어합니다 (`ZoomFactor`).
* **프로그래밍 방식 제어**: 프로그래밍 방식으로 확대/축소, 이동 및 뷰 리셋이 가능합니다.
* **이벤트 알림**:
    * `ZoomChanged`: 확대/축소 레벨이 변경될 때 발생합니다. ("줌 변경 시 발생")
    * `MousePositionChanged`: 마우스가 이미지 위에서 움직일 때 원본 이미지 내의 정확한 픽셀 좌표를 제공하며 발생합니다. ("마우스 이미지 좌표 이동 시 발생")
* **연결된 컨트롤**: 여러 `Zoombelievable` 컨트롤을 연결하여 확대/축소, 이동 및 마우스 위치를 동기화할 수 있습니다.
* **기능 활성화/비활성화**: 이동(`EnablePan`) 및 확대/축소(`EnableZoom`) 기능을 독립적으로 활성화하거나 비활성화할 수 있습니다.
* **이미지 소스 유연성**: 현재 확대/축소 및 이동 상태를 유지하면서 런타임에 표시되는 이미지를 변경할 수 있습니다 (`ImageSource`).
* **특정 지점으로 중앙 정렬**: 프로그래밍 방식으로 이미지 내의 특정 지점에 뷰를 중앙 정렬할 수 있습니다.
* **픽셀 단위 마우스 추적**: `Stretch.Uniform` 상태에서도 원본 이미지에 대한 마우스 좌표를 정확하게 결정합니다.

## 동작 방식

`Zoombelievable` 컨트롤은 `Image` 컨트롤을 포함하는 `Border` 요소입니다. 변환은 확대/축소를 위한 `ScaleTransform`과 이동(패닝)을 위한 `TranslateTransform`을 포함하는 `TransformGroup`을 사용하여 적용됩니다. 마우스 이벤트 핸들러는 이러한 변환을 대화형으로 업데이트하는 데 사용됩니다.

이 컨트롤의 핵심적인 측면 중 하나는 `Image` 컨트롤의 `Stretch.Uniform` 속성을 고려하여 원본 이미지 픽셀에 대한 마우스 위치를 계산하는 기능입니다. 이를 통해 정확한 좌표 보고 및 마우스 포인터 중심의 확대/축소와 같은 기능이 가능합니다.

연결 메커니즘은 연결된 인스턴스 간에 확대/축소, 이동 및 마우스 위치 변경 사항을 전파하며, 내부 `_isPropagatingEvent` 플래그를 사용하여 이벤트 재귀를 방지합니다.

## 속성 (Properties)

다음은 사용할 수 있는 주요 의존성 속성입니다:

* **`ImageSource`**: `ImageSource` - 표시할 이미지의 소스입니다.
* **`MinZoom`**: `double` (기본값: `0.1`) - 허용되는 최소 확대/축소 레벨입니다.
* **`MaxZoom`**: `double` (기본값: `10.0`) - 허용되는 최대 확대/축소 레벨입니다.
* **`ZoomFactor`**: `double` (기본값: `1.2`) - 각 마우스 휠 스크롤 시 확대/축소 레벨이 변경되는 비율입니다.
* **`CurrentZoom`**: `double` (기본값: `1.0`) - 현재 확대/축소 레벨입니다. 프로그래밍 방식으로 확대/축소하도록 설정할 수 있습니다.
* **`PanOffsetX`**: `double` (기본값: `0.0`) - 현재 가로 이동(패닝) 오프셋입니다. 프로그래밍 방식으로 이동하도록 설정할 수 있습니다.
* **`PanOffsetY`**: `double` (기본값: `0.0`) - 현재 세로 이동(패닝) 오프셋입니다. 프로그래밍 방식으로 이동하도록 설정할 수 있습니다.
* **`EnablePan`**: `bool` (기본값: `true`) - 마우스 이동(패닝)을 활성화하거나 비활성화합니다.
* **`EnableZoom`**: `bool` (기본값: `true`) - 마우스 휠 확대/축소를 활성화하거나 비활성화합니다.

### 읽기 전용 속성

* **`LastMouseImagePosition`**: `Point` - 원본 이미지의 픽셀 공간 내에서 마지막으로 알려진 마우스 좌표를 가져옵니다. ("마우스가 가리키는 마지막 이미지 내부 픽셀 좌표")

## 이벤트 (Events)

* **`ZoomChanged`**: `EventHandler<ZoomChangedEventArgs>`
    * `CurrentZoom` 속성이 변경될 때 발생합니다.
    * `ZoomChangedEventArgs`는 `OldZoom` 및 `NewZoom` 값을 제공합니다.
* **`MousePositionChanged`**: `EventHandler<MousePositionChangedEventArgs>`
    * 마우스가 이미지 위에서 움직이고 계산된 이미지 픽셀 좌표가 변경될 때 발생합니다.
    * `MousePositionChangedEventArgs`는 `ImagePoint` (원본 이미지에서의 마우스 좌표)를 제공합니다.

## 공개 메서드 (Public Methods)

* **`void LinkWith(Zoombelievable other)`**:
    이 컨트롤을 다른 `Zoombelievable` 컨트롤과 연결합니다. 한 컨트롤의 확대/축소, 이동 또는 마우스 위치 변경 사항이 다른 컨트롤에 반영됩니다.
* **`void UnlinkFrom(Zoombelievable other)`**:
    지정된 `Zoombelievable` 컨트롤과의 연결을 제거합니다.
* **`void UnlinkAll()`**:
    이 컨트롤이 다른 `Zoombelievable` 컨트롤과 가진 모든 연결을 제거합니다.
* **`void ResetZoom()`**:
    확대/축소를 `1.0`으로, 이동(패닝) 오프셋을 `0,0`으로 초기화합니다. ("줌과 패닝을 초기 상태(Zoom = 1.0, Pan = 0,0)로 리셋하고 이벤트를 발생시킵니다.")
* **`void ZoomIn(double factor = 0)`**:
    프로그래밍 방식으로 확대합니다. `factor`가 `0` 이하이면 `ZoomFactor` 속성을 사용합니다.
* **`void ZoomOut(double factor = 0)`**:
    프로그래밍 방식으로 축소합니다. `factor`가 `0` 이하이면 `ZoomFactor` 속성을 사용합니다.
* **`void CenterOn(Point imagePoint)`**:
    지정된 `imagePoint`로 뷰를 중앙 정렬합니다. 포인트는 원본 이미지의 좌표계 기준이어야 합니다.

## 기본 사용 예제

### XAML

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
        xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
        xmlns:local="clr-namespace:Zoombelievable;assembly=Zoombelievable" Title="Zoombelievable 데모" Height="450" Width="800">
    <Grid>
        <local:Zoombelievable x:Name="myZoombelievableControl"
                              ImageSource="your_image.png"
                              MinZoom="0.5"
                              MaxZoom="5.0"
                              ZoomFactor="1.1"
                              EnablePan="True"
                              EnableZoom="True"/>
    </Grid>
</Window>
```

### C# (이벤트 또는 프로그래밍 방식 제어를 위한 코드 비하인드 예제)

```csharp
using System.Windows;
using Zoombelievable; // 사용하는 네임스페이스

namespace YourApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 예제: 이벤트 연결
            myZoombelievableControl.ZoomChanged += OnZoomChanged;
            myZoombelievableControl.MousePositionChanged += OnMouseImagePositionChanged;
        }

        private void OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            // 줌 변경 처리, 예: 상태 표시줄 업데이트
            // System.Diagnostics.Debug.WriteLine($"줌 변경: {e.OldZoom} -> {e.NewZoom}");
        }

        private void OnMouseImagePositionChanged(object sender, MousePositionChangedEventArgs e)
        {
            // 마우스 위치 변경 처리, 예: 픽셀 좌표 표시
            // System.Diagnostics.Debug.WriteLine($"이미지 위 마우스 픽셀: {e.ImagePoint}");
        }

        // 예제: 프로그래밍 방식 제어
        private void SomeButton_Click(object sender, RoutedEventArgs e)
        {
            // myZoombelievableControl.ResetZoom();
            // myZoombelievableControl.ZoomIn();
            // myZoombelievableControl.CenterOn(new Point(100, 150)); // 이미지의 픽셀 (100,150) 지점으로 중앙 정렬
        }

        // 예제: 두 컨트롤 연결 (myZoombelievableControl2 라는 다른 컨트롤이 있다고 가정)
        // public void LinkControls()
        // {
        //     if (myZoombelievableControl != null && myZoombelievableControl2 != null)
        //     {
        //         myZoombelievableControl.LinkWith(myZoombelievableControl2);
        //     }
        // }
    }
}
```