﻿using System;
using System.Collections.Generic;
using EasyBudget.Business;
using EasyBudget.Business.ViewModels;
using EasyBudget.Models;
using Xamarin.Forms;

namespace EasyBudget.Forms.Pages
{
    public partial class CheckingAccountView : ContentPage
    {
        EasyBudgetDataService ds;
        BankAccountViewModel vm;
        int bankAccountId;

        public CheckingAccountView(int accountId)
        {
            InitializeComponent();
            ds = EasyBudgetDataService.Instance;
            bankAccountId = accountId;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            vm = await ds.GetBankAccountViewModelAsync(bankAccountId, BankAccountType.Checking);

        }
    }
}