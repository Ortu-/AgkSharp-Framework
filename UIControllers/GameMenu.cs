using AgkSharp;
using System;

namespace AGKCore.UI
{
    public class GameMenuController : UI.Controller
    {
        public GameMenuController()
        {
            Dispatcher.Add(GameMenuController.MenuOnPress);
            Dispatcher.Add(GameMenuController.ResumeOnPress);
            Dispatcher.Add(GameMenuController.QuitOnPress);
            UserInterface.LoadPartial("media/ui/partials/gameMenu.json");
        }

        public static void MenuOnPress(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            if(tElement.Style.Display == "hidden")
            {
                tElement.Style.SetProp("display", "visible");
            }
            else
            {
                tElement.Style.SetProp("display", "hidden");
            }
        }

        public static void ResumeOnPress(object rArgs)
        {
            var tElement = UI.UserInterface.GetElementById("gameMenu");
            tElement.Style.SetProp("display", "hidden");
        }

        public static void QuitOnPress(object rArgs)
        {
            App.StopRunning(false);
        }

    }

}
