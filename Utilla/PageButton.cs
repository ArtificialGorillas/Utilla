using System;

namespace Utilla
{
    public class PageButton : GorillaPressableButton
    {
        public new Action onPressed;

        public override void ButtonActivation()
        {
            base.ButtonActivation();

            onPressed();
        }
    }
}
