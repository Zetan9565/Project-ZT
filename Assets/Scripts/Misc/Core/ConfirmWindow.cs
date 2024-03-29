﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.UI
{
    public class ConfirmWindow : Window
    {
        [SerializeField]
        private Text dialogText;
        [SerializeField]
        private Button yes;
        [SerializeField]
        private Button no;

        private Action onYesClick;
        private Action onNoClick;

        protected override void OnAwake()
        {
            yes.onClick.AddListener(Confirm);
            no.onClick.AddListener(Cancel);
        }

        public static ConfirmWindow StartConfirm(string dialog)
        {
            return StartConfirm(dialog, null);
        }
        public static ConfirmWindow StartConfirm(string dialog, Action yesAction)
        {
            return StartConfirm(dialog, yesAction, null);
        }
        public static ConfirmWindow StartConfirm(string dialog, Action yesAction, Action noAction)
        {
            return WindowsManager.OpenWindow<ConfirmWindow>(dialog, yesAction, noAction);
        }

        public void Confirm()
        {
            WindowsManager.CloseWindow<ConfirmWindow>();
            onYesClick?.Invoke();
        }

        public void Cancel()
        {
            Close();
            onNoClick?.Invoke();
        }

        protected override bool OnOpen(params object[] args)
        {
            if (args != null && args.Length > 2)
            {
                dialogText.text = args[0] as string;
                onYesClick = args[1] as Action;
                onNoClick = args[2] as Action;
                Utility.SetActive(no, onYesClick != null || onNoClick != null);
                return true;
            }
            return false;
        }
    }
}