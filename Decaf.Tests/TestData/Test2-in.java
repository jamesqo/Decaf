// https://stackoverflow.com/a/8450316/4077294
switch (getResources().getConfiguration().orientation){
    case Configuration.ORIENTATION_PORTRAIT:
        if(android.os.Build.VERSION.SDK_INT < android.os.Build.VERSION_CODES.FROYO){
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        } else {
            int rotation = getWindowManager().getDefaultDisplay().getRotation();
            if(rotation == android.view.Surface.ROTATION_90|| rotation == android.view.Surface.ROTATION_180){
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_REVERSE_PORTRAIT);
            } else {
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
            }
        }   
    break;

    case Configuration.ORIENTATION_LANDSCAPE:
        if(android.os.Build.VERSION.SDK_INT < android.os.Build.VERSION_CODES.FROYO){
            setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
        } else {
            int rotation = getWindowManager().getDefaultDisplay().getRotation();
            if(rotation == android.view.Surface.ROTATION_0 || rotation == android.view.Surface.ROTATION_90){
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
            } else {
                setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_REVERSE_LANDSCAPE);
            }
        }
    break;
}