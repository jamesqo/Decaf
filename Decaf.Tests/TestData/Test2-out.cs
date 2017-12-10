// https://stackoverflow.com/a/8450316/4077294
switch (Resources.Configuration.Orientation)
{
    case Configuration.OrientationPortrait:
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Froyo)
        {
            RequestedOrientation = ActivityInfo.ScreenOrientationPortrait;
        }
        else
        {
            var rotation = WindowManager.DefaultDisplay.Rotation;
            if (rotation == Android.View.Surface.Rotation90 || rotation == SurfaceOrientation.Rotation180)
            {
                RequestedOrientation = ActivityInfo.ScreenOrientationReversePortrait;
            }
            else
            {
                RequestedOrientation = ActivityInfo.ScreenOrientationPortrait;
            }
        }
        break;
    case Configuration.OrientationLandscape:
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Froyo)
        {
            RequestedOrientation = ActivityInfo.ScreenOrientationLandscape;
        }
        else
        {
            var rotation = WindowManager.DefaultDisplay.Rotation;
            if (rotation == Android.View.Surface.Rotation0 || rotation == SurfaceOrientation.Rotation90)
            {
                RequestedOrientation = ActivityInfo.ScreenOrientationLandscape;
            }
            else
            {
                RequestedOrientation = ActivityInfo.ScreenOrientationReverseLandscape;
            }
        }
        break;
}