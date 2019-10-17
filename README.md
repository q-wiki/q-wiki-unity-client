# Q-Wiki Unity Client

A cooperation between HTW Berlin and Wikimedia Germany.

## Getting Started

To get the frontend of the project up an running, you need to follow the steps below.

### Prerequisites

You have to install the latest version of Unity3D on your computer.<br/>
We use <b>2019.1.2f1</b> for development, but more recent versions should be fine as well.

### Installing

Clone this repository.

```
git clone https://github.com/q-wiki/q-wiki-unity-client.git
```

When finished, open the project in Unity and let it import all the relevant assets.
Q-Wiki is an Android application. Change the target platform to Android. The assets may have to be reimported, which can take some time.

```
File -> Build Settings -> Android -> Switch Platform
```

That's it. The project should be up and running.<br/>
You can test it by hitting the Play button in Unity or debugging it via Visual Studio / Rider.<br/>
Make sure to pick the right scene, otherwise it won't work.

```
Assets/Scenes/Final/OpeningScene.unity
```

## Building / Testing

To build the project and test it on your phone, you can generate an .apk in Unity.

```
File -> Build Settings -> Build
File -> Build Settings -> Build and Run
```

## Contributing

If you want to contribute, make sure to follow our rules and guidelines (which will be created later on).

## License

This project is licensed under the GPL-3 License - see the [LICENSE.md](LICENSE.md) file for details
