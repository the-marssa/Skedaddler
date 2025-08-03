using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace OnScreenControls
{
	[RequireComponent (typeof (Image))]
    [RequireComponent(typeof(Animator))]
    public class Button : MonoBehaviour , IPointerUpHandler , IPointerDownHandler 
    {

        /// <summary>
        /// The name of the button in the unity input settings
        /// </summary>
        public string ButtonName = "Fire1";


        private bool _disable = false;
        public bool disable 
        {
            get{return _disable;}
            set{
                _disable = value;

                if (value)
                {
                    Anim.SetTrigger("Disabled");
                }
            }
        }

        //Animation of the Button
        private Animator Anim;

        private CrossPlatformInputManager.VirtualButton VirtualButton; 

        void Start()
        {
            VirtualButton = new CrossPlatformInputManager.VirtualButton(ButtonName);

            if (CrossPlatformInputManager.ButtonExists(VirtualButton.name))
            {
                CrossPlatformInputManager.UnRegisterVirtualButton(VirtualButton.name);
            }

            CrossPlatformInputManager.RegisterVirtualButton(VirtualButton);

            Anim = GetComponent<Animator>();

            if (disable)
            {
                Anim.SetTrigger("Disabled");
            }
            else
            {
                Anim.SetTrigger("Normal");
            }

        }

        public void OnPointerUp(PointerEventData data)
        {

            if (disable)
            {
                return;
            }

            //set the value of the button
            CrossPlatformInputManager.SetButtonUp(ButtonName);

            Anim.SetTrigger("Normal");

        }


        public void OnPointerDown (PointerEventData data) 
        {
            if (disable)
            {
                return;
            }

            //set the value of the button
            CrossPlatformInputManager.SetButtonDown(ButtonName);

            Anim.SetTrigger("Pressed");

        }


    }
}


