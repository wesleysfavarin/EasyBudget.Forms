﻿using System;
using System.Collections.Generic;
using EasyBudget.Business.ViewModels;
using Xamarin.Forms;

namespace EasyBudget.Forms.Pages
{
    public partial class SavingsDepositView : ContentPage
    {
        SavingsDepositViewModel vm;

        public SavingsDepositView()
        {
            InitializeComponent();
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();
            vm = this.BindingContext as SavingsDepositViewModel;
		}

		public async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected async void OnDepositEditTapped(object sender, TappedEventArgs e)
        {
            SavingsDepositEdit editor = new SavingsDepositEdit();
            editor.BindingContext = vm;
            await Navigation.PushAsync(editor);
        }
    }
}
