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

                tElement = UserInterface.GetElementById("btn-resume");
                tElement.Style.SetProp("opacity", "1");
                var tTran = new UI.TransitionData()
                {
                    Element = tElement,
                    InitialValue = "1",
                    TargetValue = "100",
                    Property = "opacity",
                    Duration = 1200,
                    Start = App.Timing.Timer
                };
                UserInterface.TransitionList.Add(tTran);
                tTran = new UI.TransitionData()
                {
                    Element = tElement,
                    InitialValue = "-1000px",
                    TargetValue = tElement.Style.Top,
                    Property = "top",
                    Duration = 800,
                    Start = App.Timing.Timer
                };
                UserInterface.TransitionList.Add(tTran);

                tElement = UserInterface.GetElementById("btn-close");
                tElement.Style.SetProp("opacity", "1");
                tTran = new UI.TransitionData()
                {
                    Element = tElement,
                    InitialValue = "1",
                    TargetValue = "100",
                    Property = "opacity",
                    Duration = 1200,
                    Start = App.Timing.Timer
                };
                UserInterface.TransitionList.Add(tTran);
                tTran = new UI.TransitionData()
                {
                    Element = tElement,
                    InitialValue = "1000px",
                    TargetValue = tElement.Style.Top,
                    Property = "top",
                    Duration = 800,
                    Start = App.Timing.Timer
                };
                UserInterface.TransitionList.Add(tTran);
            }
            else
            {
                tElement.Style.SetProp("display", "hidden");
            }
        }

        public static void ResumeOnPress(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            tElement.RemoveStyleClass(tElement.Name + "-push");

            tElement = UI.UserInterface.GetElementById("gameMenu");
            tElement.Style.SetProp("display", "hidden");
        }

        public static void QuitOnPress(object rArgs)
        {
            App.StopRunning(false);
        }

    }

}
