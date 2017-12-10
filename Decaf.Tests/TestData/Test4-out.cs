// https://stackoverflow.com/a/151940/4077294
public override void OnSaveInstanceState(Bundle savedInstanceState)
{
    base.OnSaveInstanceState(savedInstanceState);
    // Save UI state changes to the savedInstanceState.
    // This bundle will be passed to onCreate if the process is
    // killed and restarted.
    savedInstanceState.PutBoolean(""MyBoolean"", true);
    savedInstanceState.PutDouble(""myDouble"", 1.9);
    savedInstanceState.PutInt(""MyInt"", 1);
    savedInstanceState.PutString(""MyString"", ""Welcome back to Android"");
    // etc.
}

public override void OnRestoreInstanceState(Bundle savedInstanceState)
{
    base.OnRestoreInstanceState(savedInstanceState);
    // Restore UI state from the savedInstanceState.
    // This bundle has also been passed to onCreate.
    bool myBoolean = savedInstanceState.GetBoolean(""MyBoolean"");
    double myDouble = savedInstanceState.GetDouble(""myDouble"");
    int myInt = savedInstanceState.GetInt(""MyInt"");
    string myString = savedInstanceState.GetString(""MyString"");
}