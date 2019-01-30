using AgkSharp;
using System;

namespace AGKCore.UI
{
    public class CommonController : UI.Controller
    {
        public CommonController()
        {
            Dispatcher.Add(CommonController.DoHoverByType);
            Dispatcher.Add(CommonController.DoUnHoverByType);
            Dispatcher.Add(CommonController.DoPressByType);
            Dispatcher.Add(CommonController.DoReleaseByType);
            UserInterface.LoadPartial("media/ui/partials/common.json");
        }

        public static void DoHoverByType(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            switch (tElement.Tag)
            {
                case "button":
                    tElement.AddStyleClass(tElement.Name + "-pop");
                    break;
            }
        }

        public static void DoUnHoverByType(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            switch (tElement.Tag)
            {
                case "button":
                    tElement.RemoveStyleClass(tElement.Name + "-pop");
                    break;
            }
        }

        public static void DoPressByType(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            switch (tElement.Tag)
            {
                case "button":
                    tElement.AddStyleClass(tElement.Name + "-push");
                    break;
            }
        }

        public static void DoReleaseByType(object rArgs)
        {
            var tElement = UI.UserInterface.ElementList[Convert.ToInt32(rArgs)];
            switch (tElement.Tag)
            {
                case "button":
                    tElement.RemoveStyleClass(tElement.Name + "-push");
                    break;
            }
        }

    }

}
