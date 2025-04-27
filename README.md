# InkAR - AR Tattoo Visualization Hub

**InkAR** is an augmented reality (AR) hub that allows users to try on tattoos in real time. The app supports uploading, browsing, and viewing custom tattoo designs in AR, providing a realistic experience for potential customers.

## Features

- **Browse and try on tattoos in AR**: Users can view tattoos on their body using AR.
- **Artist support**: Tattoo artists can upload their designs and share them with customers. Tattoos are converted from PNG to AR objects automatically.
- **Scaling**: Users can scale tattoos in real-time.
- **Screenshot functionality**: Hide UI and capture images of placement and save them to the gallery.
- **Communication**: Send an email with our template directly to artists with the click of a button.

---

## Installation Guide

### Android

**Minimum Requirements** - This app requires Android 7.0 (API level 24) or higher and ARCore. The list of devices that support ARCore can be found here - https://developers.google.com/ar/devices

1. **Download the APK from GitHub Releases**  
   Go to [inkAR v1.0.0](https://github.com/CianCicero/inkAR/releases/tag/v1.0.0) and download APK file.

2. **Install the APK on Your Android Device**
   - Transfer the downloaded `.apk` file to your Android device.
   - On your Android device, open the file using your file manager.
   - Follow the prompts to install the APK.

3. **Allow Installation from Unknown Sources**
   If you haven't installed APKs from external sources before, you need to allow installations from unknown sources in your device's settings:
   - Go to **Settings** > **Security** (or **Apps & Notifications** on newer versions).
   - Enable **Install unknown apps** for your file manager or browser app.

4. **Locate and run InkAR**


### Unity 

**Note**: Firebase credentials are not included in this repository due to security reasons. Running this application via Unity requires the creation and integration of your own Firebase elements.

1. **Clone the repository**:
   Clone this repository to your local machine:
   ```bash
   git clone https://github.com/CianCicero/inkAR.git

2. **Open the project in Unity**: This project is compatible with Unity Editor v6 and above
3. **Install Firebase SDK**: This project requires Firebase Storage, Firetore, and Authentication. The SDKs are not included in the repo due to their size. Follow Firebase's guide to instal the required SDKs - https://firebase.google.com/docs/unity/setup
4. **Integrate your Firebase elements by following the [guide](https://firebase.google.com/docs/unity/setup)** - Requires a user table, tattoometa table, tattoos bucket, and authentication.
