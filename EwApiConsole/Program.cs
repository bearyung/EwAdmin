﻿using EwApiConsole;
using Terminal.Gui;

Application.Init();

try
{
    Application.Run(new MyView());
}
finally
{
    Application.Shutdown();
}