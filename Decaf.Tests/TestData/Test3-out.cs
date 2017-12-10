// https://stackoverflow.com/a/1109108/4077294
// Check if no view has focus:
var view = this.CurrentFocus;
if (view != null)
{
    var imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
    imm.HideSoftInputFromWindow(view.WindowToken, 0);
}