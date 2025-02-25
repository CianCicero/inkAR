# inkAR

**inkAR** is a tattoo visualization application built with Unity and AR Foundation. It enables tattoo artists to upload custom designs, allowing users to see how the tattoos would look on their own bodies using Augmented Reality.

## Features

- **Image Tracking:** Detects and tracks reference images to position tattoos accurately.
- **Live Customization:** Users can scale, rotate, and bend the tattoo design in real-time.
- **Touch-Based Transformations:** Intuitive touch gestures for manipulating tattoos in AR.

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/CianCicero/inkAR.git
   cd inkAR
   ```
2. Open the project in Unity (tested with Unity 2022+).
3. Make sure the **AR Foundation** and **ARKit/ARCore** packages are installed via the Package Manager.
4. Build and run the app on a compatible device (iOS or Android).

## Usage

1. Launch the app and point your camera at a reference image.
2. The tattoo design will appear in AR, anchored to the tracked image.
3. Use sliders to adjust the size, bend, and color of the design.
4. Preview the tattoo from different angles in real-time.

## Roadmap

- [ ] Add user GUI for browsing and saving designs.
- [ ] Allow for mesh manipulation.
- [ ] Add artist GUI for uploading designs.

