switch (foo)
{
    case 1:
        Bar();
        goto case 2;
    case 2:
        Baz();
        goto default;
    default:
        Bag();
        break;
}