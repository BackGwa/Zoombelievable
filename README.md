# Zoombelievable

Zoombelievable is a custom WPF control that provides interactive zooming and panning capabilities for images. It's designed to be easy to use and integrate into WPF applications, offering a smooth user experience for viewing detailed images. The control also supports linking multiple instances together for synchronized transformations.

## Features

* **Mouse Wheel Zooming**: Zoom in and out of the image using the mouse wheel. The zoom is centered on the mouse cursor's position.
* **Mouse Drag Panning**: Pan the image by clicking and dragging with the left mouse button.
* **Configurable Zoom Limits**: Set minimum and maximum zoom levels (`MinZoom`, `MaxZoom`).
* **Adjustable Zoom Speed**: Control the zoom increment/decrement factor (`ZoomFactor`).
* **Programmatic Control**: Zoom, pan, and reset the view programmatically.
* **Event Notifications**:
    * `ZoomChanged`: Fires when the zoom level changes.
    * `MousePositionChanged`: Fires when the mouse moves over the image, providing the exact pixel coordinates within the original image.
* **Linked Controls**: Link multiple `Zoombelievable` controls to synchronize their zoom, pan, and mouse position.
* **Enable/Disable Functionality**: Independently enable or disable panning (`EnablePan`) and zooming (`EnableZoom`).
* **Image Source Flexibility**: Change the displayed image (`ImageSource`) at runtime while maintaining the current zoom and pan state.
* **Center On Point**: Programmatically center the view on a specific point within the image.
* **Pixel-Perfect Mouse Tracking**: Accurately determines mouse coordinates on the original image, even with `Stretch.Uniform`.

## How It Works

The `Zoombelievable` control is a `Border` element that contains an `Image` control. Transformations are applied using a `TransformGroup` which includes a `ScaleTransform` for zooming and a `TranslateTransform` for panning. Mouse event handlers are used to update these transformations interactively.

A key aspect of the control is its ability to calculate the mouse's position relative to the original image pixels, taking into account the `Stretch.Uniform` property of the `Image` control. This allows for accurate coordinate reporting and features like zooming towards the mouse pointer.

The linking mechanism propagates zoom, pan, and mouse position changes between linked instances, using an internal `_isPropagatingEvent` flag to prevent event recursion.

## Properties

Here are the key dependency properties you can use:

* **`ImageSource`**: `ImageSource` - The source of the image to be displayed.
* **`MinZoom`**: `double` (Default: `0.1`) - The minimum allowed zoom level.
* **`MaxZoom`**: `double` (Default: `10.0`) - The maximum allowed zoom level.
* **`ZoomFactor`**: `double` (Default: `1.2`) - The factor by which the zoom level changes with each mouse wheel increment.
* **`CurrentZoom`**: `double` (Default: `1.0`) - The current zoom level. Can be set to programmatically zoom.
* **`PanOffsetX`**: `double` (Default: `0.0`) - The current horizontal pan offset. Can be set to programmatically pan.
* **`PanOffsetY`**: `double` (Default: `0.0`) - The current vertical pan offset. Can be set to programmatically pan.
* **`EnablePan`**: `bool` (Default: `true`) - Enables or disables mouse panning.
* **`EnableZoom`**: `bool` (Default: `true`) - Enables or disables mouse wheel zooming.

### Read-Only Properties

* **`LastMouseImagePosition`**: `Point` - Gets the last known mouse coordinates within the original image's pixel space.

## Events

* **`ZoomChanged`**: `EventHandler<ZoomChangedEventArgs>`
    * Occurs when the `CurrentZoom` property changes.
    * `ZoomChangedEventArgs` provides `OldZoom` and `NewZoom` values.
* **`MousePositionChanged`**: `EventHandler<MousePositionChangedEventArgs>`
    * Occurs when the mouse moves over the image and the calculated image pixel coordinate changes.
    * `MousePositionChangedEventArgs` provides the `ImagePoint` (mouse coordinates on the original image).

## Public Methods

* **`void LinkWith(Zoombelievable other)`**:
    Links this control with another `Zoombelievable` control. Changes in zoom, pan, or mouse position in one control will be reflected in the other.
* **`void UnlinkFrom(Zoombelievable other)`**:
    Removes the link with the specified `Zoombelievable` control.
* **`void UnlinkAll()`**:
    Removes all links this control has with other `Zoombelievable` controls.
* **`void ResetZoom()`**:
    Resets the zoom to `1.0` and pan offsets to `0,0`.
* **`void ZoomIn(double factor = 0)`**:
    Programmatically zooms in. If `factor` is `0` or less, it uses the `ZoomFactor` property.
* **`void ZoomOut(double factor = 0)`**:
    Programmatically zooms out. If `factor` is `0` or less, it uses the `ZoomFactor` property.
* **`void CenterOn(Point imagePoint)`**:
    Centers the view on the specified `imagePoint`. The point should be in the coordinate system of the original image.

## Basic Usage Example

### XAML

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="[http://schemas.microsoft.com/winfx/2006/xaml/presentation](http://schemas.microsoft.com/winfx/2006/xaml/presentation)"
        xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
        xmlns:local="clr-namespace:Zoombelievable;assembly=Zoombelievable" Title="Zoombelievable Demo" Height="450" Width="800">
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

### C# (Code-behind example for events or programmatic control)

```csharp
using System.Windows;
using Zoombelievable; // Your namespace

namespace YourApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Example: Hook into events
            myZoombelievableControl.ZoomChanged += OnZoomChanged;
            myZoombelievableControl.MousePositionChanged += OnMouseImagePositionChanged;
        }

        private void OnZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            // Handle zoom change, e.g., update a status bar
            // System.Diagnostics.Debug.WriteLine($"Zoom changed from {e.OldZoom} to {e.NewZoom}");
        }

        private void OnMouseImagePositionChanged(object sender, MousePositionChangedEventArgs e)
        {
            // Handle mouse position change, e.g., display pixel coordinates
            // System.Diagnostics.Debug.WriteLine($"Mouse at image pixel: {e.ImagePoint}");
        }

        // Example: Programmatic control
        private void SomeButton_Click(object sender, RoutedEventArgs e)
        {
            // myZoombelievableControl.ResetZoom();
            // myZoombelievableControl.ZoomIn();
            // myZoombelievableControl.CenterOn(new Point(100, 150)); // Center on pixel (100,150) of the image
        }

        // Example: Linking two controls (assuming you have another control named 'myZoombelievableControl2')
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