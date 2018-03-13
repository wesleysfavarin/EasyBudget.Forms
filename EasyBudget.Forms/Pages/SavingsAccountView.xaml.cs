﻿using System;
using System.Collections.Generic;
using EasyBudget.Business;
using EasyBudget.Business.ViewModels;
using EasyBudget.Forms.Utility;
using EasyBudget.Models;
using Xamarin.Forms;

namespace EasyBudget.Forms.Pages
{
    public partial class SavingsAccountView : ContentPage
    {
        BankAccountViewModel vm;

        public SavingsAccountView()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            vm = (this.BindingContext as BankAccountViewModel);
            var chart = await ChartUtility.Instance.GetChartAsync(vm);
            chartAccountSummary.Chart = chart;
            //vm.SelectedRegisterItem = null;
        }

        public async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        public async void btnNewDeposit_Clicked(object sender, EventArgs eventArgs)
        {
            if (vm == null)
            {
                vm = this.BindingContext as BankAccountViewModel;
            }
            var depositVM = await vm.AddDepositAsync();
            vm.SelectedRegisterItem = depositVM;
            if (depositVM != null)
            {
                SavingsDepositEdit depositViewer = new SavingsDepositEdit();
                depositViewer.BindingContext = depositVM as SavingsDepositViewModel;
                await Navigation.PushAsync(depositViewer);
            }
        }

        public async void btnNewWithdrawal_Clicked(object sender, EventArgs eventArgs)
        {
            if (vm == null)
            {
                vm = this.BindingContext as BankAccountViewModel;
            }
            var withdrawalVM = await vm.AddWithdrawalAsync();
            vm.SelectedRegisterItem = withdrawalVM;
            if (withdrawalVM != null)
            {
                SavingsWithdrawalEdit withdrawalViewer = new SavingsWithdrawalEdit();
                withdrawalViewer.BindingContext = withdrawalVM as SavingsWithdrawalViewModel;
                await Navigation.PushAsync(withdrawalViewer);
            }
        }

        protected void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            vm.SelectedRegisterItem = e.SelectedItem as AccountRegisterItemViewModel;
        }

        protected async void OnItemEdit(object sender, EventArgs e)
        {
            var btn = sender as MenuItem;
            var regItem = btn.BindingContext as AccountRegisterItemViewModel;
            vm.SelectedRegisterItem = regItem;
            switch (regItem.ItemType)
            {
                case AccountRegisterItemViewModel.AccountItemType.Deposits:
                    SavingsDepositEdit depEditor = new SavingsDepositEdit();
                    depEditor.BindingContext = regItem as SavingsDepositViewModel;
                    await Navigation.PushAsync(depEditor);
                    break;
                case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                    SavingsWithdrawalEdit witEditor = new SavingsWithdrawalEdit();
                    witEditor.BindingContext = regItem as SavingsWithdrawalViewModel;
                    await Navigation.PushAsync(witEditor);
                    break;
            }
        }

        protected async void OnItemDelete(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Confirmation", "Are you sure you want to delete this item?", "Yes", "No");

            if (answer)
            {
                var btn = sender as MenuItem;
                var regItem = btn.BindingContext as AccountRegisterItemViewModel;
                vm.SelectedRegisterItem = regItem;

                bool deleted = false;
                switch (regItem.ItemType)
                {
                    case AccountRegisterItemViewModel.AccountItemType.Deposits:
                        deleted = await vm.DeleteRegisterItemAsync(regItem as DepositViewModel);

                        break;
                    case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                        deleted = await vm.DeleteRegisterItemAsync(regItem as WithdrawalViewModel);

                        break;
                }
                if (deleted)
                {
                    await DisplayAlert("Results", "Item Deleted", "Dismiss");
                }
                else
                {
                    await DisplayAlert("Error", "Unable to delete this Item. Message: " + regItem.ErrorCondition, "Ok");
                }
            }
        }

        public async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            var itemVM = e.Item as AccountRegisterItemViewModel;
            vm.SelectedRegisterItem = itemVM;
            switch (itemVM.ItemType)
            {
                case AccountRegisterItemViewModel.AccountItemType.Deposits:
                    SavingsDepositView depositViewer = new SavingsDepositView();
                    depositViewer.BindingContext = itemVM as SavingsDepositViewModel;
                    await Navigation.PushAsync(depositViewer);
                    break;
                case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                    SavingsDepositView withdrawalViewer = new SavingsDepositView();
                    withdrawalViewer.BindingContext = itemVM as SavingsWithdrawalViewModel;
                    await Navigation.PushAsync(withdrawalViewer);
                    break;
            }
        }
    }
}
