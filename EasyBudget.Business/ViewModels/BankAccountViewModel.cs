﻿//
//  Copyright 2018  CrawfordNET Solutions, LLC
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using EasyBudget.Models;
using EasyBudget.Models.DataModels;
using EasyBudget.Business.ChartModels;

namespace EasyBudget.Business.ViewModels
{

    public class BankAccountViewModel : BaseViewModel, INotifyPropertyChanged, IDisposable
    {
        BankAccount model { get; set; }

        public string BankName 
        {
            get
            {
                return model.bankName;
            }
            set
            {
                if (model.bankName != value)
                {
                    model.bankName = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BankName)));
                }
            }
        }

        public BankAccountType AccountType 
        {
            get
            {
                return model.accountType;
            }
            set
            {
                if (model.accountType != value)
                {
                    model.accountType = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountType)));
                }
            }
        }

        public decimal CurrentBalance 
        {
            get
            {
                return model.currentBalance;
            }
            set
            {
                if (model.currentBalance != value)
                {
                    model.currentBalance = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBalance)));
                }
            }
        }

        public string RoutingNumber 
        {
            get 
            {
                return model.routingNumber;
            }
            set
            {
                if (model.routingNumber != value)
                {
                    model.routingNumber = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoutingNumber)));
                }
            }
        }

        public string AccountNumber 
        {
            get
            {
                return model.accountNumber;
            }
            set
            {
                if (model.accountNumber != value)
                {
                    model.accountNumber = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountNumber)));
                }
            }
        }

        public string Nickname 
        {
            get
            {
                return model.accountNickname;
            }
            set
            {
                if (model.accountNickname != value)
                {
                    model.accountNickname = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nickname)));
                }
            }
        }

        public bool CanAddItems
        {
            get
            {
                return !this.IsNew;
            }
        }

        AccountRegisterItemViewModel _SelectedRegisterItem;
        public AccountRegisterItemViewModel SelectedRegisterItem
        {
            get
            {
                return _SelectedRegisterItem;
            }
            set
            {
                _SelectedRegisterItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRegisterItem)));
            }
        }

        public string ColorCode
        {
            get
            {
                return model.ColorCode;
            }
            set
            {
                if (model.ColorCode != value)
                {
                    model.ColorCode = value;
                    this.IsDirty = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorCode)));
                }
            }
        }

        public ObservableCollection<AccountRegisterItemViewModel> AccountRegister { get; set; }

        public ObservableCollection<Grouping<string, AccountRegisterItemViewModel>> AccountRegisterGrouped { get; set; }

        public decimal TotalDeposits
        {
            get
            {
                var _depositsTotal = this.AccountRegister.Where(itm => itm.ItemType == AccountRegisterItemViewModel.AccountItemType.Deposits).Sum(itm => itm.ItemAmount);
                return _depositsTotal;
            }
        }

        public decimal TotalWithdrawals
        {
            get
            {
                var _withdrawalsTotal = this.AccountRegister.Where(itm => itm.ItemType == AccountRegisterItemViewModel.AccountItemType.Withdrawals).Sum(itm => itm.ItemAmount);
                return _withdrawalsTotal;
            }
        }

        internal BankAccountViewModel(string dbFilePath)
            : base(dbFilePath)
        {
            this.AccountRegister = new ObservableCollection<AccountRegisterItemViewModel>();
            this.AccountRegisterGrouped = new ObservableCollection<Grouping<string, AccountRegisterItemViewModel>>();
        }
        
        public void GroupAccountItems()
        {
            var grouped = from regItm in this.AccountRegister
                          orderby regItm.ItemDate descending
                          group regItm by regItm.ItemType into Group
                          select new Grouping<string, AccountRegisterItemViewModel>(Group.Key.ToString(), Group);
            this.AccountRegisterGrouped = new ObservableCollection<Grouping<string, AccountRegisterItemViewModel>>(grouped.OrderBy(g => g.Key));
        }

        public async Task GroupAccountItemsAsync()
        {
            var grouped = from regItm in this.AccountRegister
                          orderby regItm.ItemDate descending
                          group regItm by regItm.ItemType into Group
                          select new Grouping<string, AccountRegisterItemViewModel>(Group.Key.ToString(), Group);

            this.AccountRegisterGrouped = await Task.Run(() => new ObservableCollection<Grouping<string, AccountRegisterItemViewModel>>(grouped.OrderBy(g => g.Key)));
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        internal async Task PopulateVMAsync(BankAccount account)
        {
            this.model = account;
            this.AccountRegister = new ObservableCollection<AccountRegisterItemViewModel>();
            this.AccountRegisterGrouped = new ObservableCollection<Grouping<string, AccountRegisterItemViewModel>>();
            await LoadDepositsAsync(false);
            await LoadWithdrawalsAsync(false);
            await GroupAccountItemsAsync();

        }

        public async Task<AccountRegisterItemViewModel> AddDepositAsync()
        {
            AccountRegisterItemViewModel vm = null;
            switch (this.AccountType)
            {
                case BankAccountType.Checking:
                    vm = await AddCheckingDepositAsync();
                    (vm as CheckingDepositViewModel).ItemUpdated += OnRegisterUpdated;
                    break;
                case BankAccountType.Savings:
                    vm = await AddSavingsDepositAsync();
                    (vm as SavingsDepositViewModel).ItemUpdated += OnRegisterUpdated;
                    break;
            }

            this.AccountRegister.Add(vm);
            await GroupAccountItemsAsync();
            this.SelectedRegisterItem = vm;

            return vm;
        }

        public async Task<AccountRegisterItemViewModel> AddWithdrawalAsync()
        {
            AccountRegisterItemViewModel vm = null;
            switch (this.AccountType)
            {
                case BankAccountType.Checking:
                    vm = await AddCheckingWithdrawalAsync();
                    (vm as CheckingWithdrawalViewModel).ItemUpdated += OnRegisterUpdated;
                    break;
                case BankAccountType.Savings:
                    vm = await AddSavingsWithdrawalAsync();
                    (vm as SavingsWithdrawalViewModel).ItemUpdated += OnRegisterUpdated;
                    break;
            }

            this.AccountRegister.Add(vm);
            await GroupAccountItemsAsync();
            this.SelectedRegisterItem = vm;

            return vm;
        }

        async Task<CheckingDepositViewModel> AddCheckingDepositAsync()
        {
            CheckingDepositViewModel vm = new CheckingDepositViewModel(this.dbFilePath);
            vm.IsNew = true;
            vm.CanEdit = true;
            vm.CanDelete = false;
            vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Deposits;

            CheckingDeposit deposit = new CheckingDeposit();
            deposit.checkingAccount = model as CheckingAccount;
            deposit.checkingAccountId = model.id;

            await vm.PopulateVMAsync(deposit);

            return vm;
        }

        async Task<SavingsDepositViewModel> AddSavingsDepositAsync()
        {
            SavingsDepositViewModel vm = new SavingsDepositViewModel(this.dbFilePath);
            vm.IsNew = true;
            vm.CanEdit = true;
            vm.CanDelete = false;
            vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Deposits;

            SavingsDeposit deposit = new SavingsDeposit();
            deposit.savingsAccount = model as SavingsAccount;
            deposit.savingsAccountId = model.id;
            deposit.transactionDate = DateTime.Now;

            await vm.PopulateVMAsync(deposit);

            //this.AccountRegister.Add(vm);
            //await GroupAccountItemsAsync();
            //this.SelectedRegisterItem = vm;

            return vm;
        }

        async Task<CheckingWithdrawalViewModel> AddCheckingWithdrawalAsync()
        {
            CheckingWithdrawalViewModel vm = new CheckingWithdrawalViewModel(this.dbFilePath);
            vm.IsNew = true;
            vm.CanEdit = true;
            vm.CanDelete = false;
            vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Withdrawals;

            CheckingWithdrawal withdrawal = new CheckingWithdrawal();
            withdrawal.checkingAccount = model as CheckingAccount;
            withdrawal.checkingAccountId = model.id;
            withdrawal.transactionDate = DateTime.Now;
            await vm.PopulateVMAsync(withdrawal);

            //this.AccountRegister.Add(vm);
            //await GroupAccountItemsAsync();
            //this.SelectedRegisterItem = vm;

            return vm;
        }

        async Task<SavingsWithdrawalViewModel> AddSavingsWithdrawalAsync()
        {
            SavingsWithdrawalViewModel vm = new SavingsWithdrawalViewModel(this.dbFilePath);
            vm.IsNew = true;
            vm.CanEdit = true;
            vm.CanDelete = false;
            vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Withdrawals;

            SavingsWithdrawal withdrawal = new SavingsWithdrawal();
            withdrawal.savingsAccount = model as SavingsAccount;
            withdrawal.savingsAccountId = model.id;
            withdrawal.transactionDate = DateTime.Now;
            await vm.PopulateVMAsync(withdrawal);

            //this.AccountRegister.Add(vm);
            //await GroupAccountItemsAsync();
            //this.SelectedRegisterItem = vm;

            return vm;
        }

        internal async Task LoadVMAsync(int accountId, BankAccountType accountType)
        {
            switch (accountType)
            {
                case BankAccountType.Checking:
                    await LoadCheckingAccountAsync(accountId);
                    break;
                case BankAccountType.Savings:
                    await LoadSavingsAccountAsync(accountId);
                    break;
            }
            await GroupAccountItemsAsync();
        }

        public async Task LoadDepositsAsync(bool getReconciled = false)
        {
            switch (this.AccountType)
            {
                case BankAccountType.Checking:
                    await LoadCheckingDepositsAsync(getReconciled);
                    break;
                case BankAccountType.Savings:
                    await LoadSavingsDepositsAsync(getReconciled);
                    break;
            }
        }

        public async Task LoadWithdrawalsAsync(bool getReconciled = false)
        {
            switch (this.AccountType)
            {
                case BankAccountType.Checking:
                    await LoadCheckingWithdrawalsAsync(getReconciled);
                    break;
                case BankAccountType.Savings:
                    await LoadSavingsWithdrawalsAsync(getReconciled);
                    break;
            }
        }

        //public async Task<ICollection<AccountRegisterItemViewModel>> GetChartData()
        //{
        //    List<AccountRegisterItemViewModel> models = new List<AccountRegisterItemViewModel>();
        //    using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
        //    {
        //        string firstOfMonthStr = DateTime.Now.Month.ToString() + "/1/" + DateTime.Now.Year.ToString();
        //        DateTime fromDate = DateTime.Parse(firstOfMonthStr);
        //        DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
        //        if (this.AccountType == BankAccountType.Checking)
        //        {
        //            var _resultsChecking = await uow.GetLoadedCheckingAccountAsync(this.model.id, fromDate, toDate);
        //            if (_resultsChecking.Successful)
        //            {
        //                foreach (CheckingDeposit dep in _resultsChecking.Results.deposits)
        //                {
        //                    var vm = new CheckingDepositViewModel(this.dbFilePath);
        //                    vm.ItemAmount = dep.transactionAmount;
        //                    vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Deposits;
        //                    vm.ItemDate = dep.transactionDate;
        //                    models.Add(vm);
        //                }
        //                foreach (CheckingWithdrawal wid in _resultsChecking.Results.withdrawals)
        //                {
        //                    var vm = new CheckingWithdrawalViewModel(this.dbFilePath);
        //                    vm.ItemAmount = wid.transactionAmount;
        //                    vm.ItemDate = wid.transactionDate;
        //                    vm.ItemType = AccountRegisterItemViewModel.AccountItemType.Withdrawals;
        //                    models.Add(vm);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // TODO Add Savings Account Logic 
        //        }
        //    }
        //    return models;
        //}

        async Task LoadCheckingAccountAsync(int accountId)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetCheckingAccountAsync(accountId);
                if (_results.Successful)
                {
                    await PopulateVMAsync(_results.Results);
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading bank account records");
                    }
                }
            }
        }

        async Task LoadSavingsAccountAsync(int accountId)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetSavingsAccountAsync(accountId);
                if (_results.Successful)
                {
                    await PopulateVMAsync(_results.Results);
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading bank account records");
                    }
                }
            }
        }

        async Task LoadCheckingDepositsAsync(bool getReconciled = false)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetCheckingDepositsAsync(model.id, getReconciled);
                if (_results.Successful)
                {
                    foreach(var deposit in _results.Results)
                    {
                        deposit.checkingAccount = model as CheckingAccount;

                        CheckingDepositViewModel vm = new CheckingDepositViewModel(this.dbFilePath);
                        vm.IsNew = false;
                        vm.CanEdit = true;
                        vm.CanDelete = true;
                        await vm.PopulateVMAsync(deposit);
                        vm.ItemUpdated += OnRegisterUpdated;
                        this.AccountRegister.Add(vm);
                    }
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading deposit records");
                    }
                }
            }
        }

        async Task LoadCheckingWithdrawalsAsync(bool getReconciled = false)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetCheckingWithdrawalsAsync(model.id, getReconciled);
                if (_results.Successful)
                {
                    foreach (var withdrawal in _results.Results)
                    {
                        withdrawal.checkingAccount = model as CheckingAccount;

                        CheckingWithdrawalViewModel vm = new CheckingWithdrawalViewModel(this.dbFilePath);
                        vm.IsNew = false;
                        vm.CanEdit = true;
                        vm.CanDelete = true;
                        await vm.PopulateVMAsync(withdrawal);
                        vm.ItemUpdated += OnRegisterUpdated;
                        this.AccountRegister.Add(vm);
                    }
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading withdrawal records");
                    }
                }
            }
        }

        async Task LoadSavingsDepositsAsync(bool getReconciled = false)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetSavingsDepositsAsync(model.id, getReconciled);
                if (_results.Successful)
                {
                    foreach (var deposit in _results.Results)
                    {
                        deposit.savingsAccount = model as SavingsAccount;

                        SavingsDepositViewModel vm = new SavingsDepositViewModel(this.dbFilePath);
                        vm.IsNew = false;
                        vm.CanEdit = true;
                        vm.CanDelete = true;
                        await vm.PopulateVMAsync(deposit);
                        vm.ItemUpdated += OnRegisterUpdated;
                        this.AccountRegister.Add(vm);
                    }
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading deposit records");
                    }
                }
            }
        }

        async Task LoadSavingsWithdrawalsAsync(bool getReconciled = false)
        {
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                var _results = await uow.GetSavingsWithdrawalsAsync(model.id, getReconciled);
                if (_results.Successful)
                {
                    foreach (var withdrawal in _results.Results)
                    {
                        withdrawal.savingsAccount = model as SavingsAccount;
                        SavingsWithdrawalViewModel vm = new SavingsWithdrawalViewModel(this.dbFilePath);
                        vm.IsNew = false;
                        vm.CanEdit = true;
                        vm.CanDelete = true;
                        await vm.PopulateVMAsync(withdrawal);
                        vm.ItemUpdated += OnRegisterUpdated;
                        this.AccountRegister.Add(vm);
                    }
                }
                else
                {
                    if (_results.WorkException != null)
                    {
                        WriteErrorCondition(_results.WorkException);
                    }
                    else if (!string.IsNullOrEmpty(_results.Message))
                    {
                        WriteErrorCondition(_results.Message);
                    }
                    else
                    {
                        WriteErrorCondition("An unknown error has occurred loading withdrawal records");
                    }
                }
            }
        }

        public async Task SaveChangesAsync()
        {
            bool _saveOk = true;
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                switch (this.AccountType)
                {
                    case BankAccountType.Checking:
                        if (this.IsNew)
                        {
                            var _resultsAddChecking = await uow.AddCheckingAccountAsync(model as CheckingAccount);
                            _saveOk = _resultsAddChecking.Successful;
                            if (_saveOk)
                            {
                                this.IsDirty = false;
                                this.IsNew = false;
                                this.CanEdit = true;
                                this.CanDelete = true;
                                model = _resultsAddChecking.Results;
                            }
                            else
                            {
                                if (_resultsAddChecking.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsAddChecking.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsAddChecking.Message))
                                {
                                    WriteErrorCondition(_resultsAddChecking.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred saving account record");
                                }
                            }
                        }
                        else
                        {
                            var _resultsUpdateChecking = await uow.UpdateCheckingAccountAsync(model as CheckingAccount);
                            _saveOk = _resultsUpdateChecking.Successful;
                            if (_saveOk)
                            {
                                this.IsDirty = false;
                                this.IsNew = false;
                                this.CanEdit = true;
                                this.CanDelete = true;
                                model = _resultsUpdateChecking.Results;
                            }
                            else
                            {
                                if (_resultsUpdateChecking.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsUpdateChecking.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsUpdateChecking.Message))
                                {
                                    WriteErrorCondition(_resultsUpdateChecking.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred saving account record");
                                }
                            }
                        }
                        break;
                    case BankAccountType.Savings:
                        if (this.IsNew)
                        {
                            var _resultsAddSavings = await uow.AddSavingsAccountAsync(model as SavingsAccount);
                            _saveOk = _resultsAddSavings.Successful;
                            if (_saveOk)
                            {
                                this.IsDirty = false;
                                this.IsNew = false;
                                this.CanEdit = true;
                                this.CanDelete = true;
                                model = _resultsAddSavings.Results;
                            }
                            else
                            {
                                if (_resultsAddSavings.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsAddSavings.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsAddSavings.Message))
                                {
                                    WriteErrorCondition(_resultsAddSavings.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred saving account record");
                                }
                            }
                        }
                        else
                        {
                            var _resultsUpdateSavings = await uow.UpdateSavingsAccountAsync(model as SavingsAccount);
                            _saveOk = _resultsUpdateSavings.Successful;
                            if (_saveOk)
                            {
                                this.IsDirty = false;
                                this.IsNew = false;
                                this.CanEdit = true;
                                this.CanDelete = true;
                                model = _resultsUpdateSavings.Results;
                            }
                            else
                            {
                                if (_resultsUpdateSavings.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsUpdateSavings.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsUpdateSavings.Message))
                                {
                                    WriteErrorCondition(_resultsUpdateSavings.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred saving account record");
                                }
                            }
                        }
                        break;
                }

                if (_saveOk)
                {
                    foreach (var item in this.AccountRegister)
                    { 
                        if (item.IsDirty)
                        {
                            switch (item.ItemType)
                            {
                                case AccountRegisterItemViewModel.AccountItemType.Deposits:
                                    await (item as DepositViewModel).SaveChangesAsync();
                                    break;
                                case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                                    await (item as WithdrawalViewModel).SaveChangesAsync();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public async Task<bool> DeleteAsync()
        {
            bool _deleted = false;

            if (this.AccountRegister.Count == 0)
            {
                using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
                {
                    switch (this.AccountType)
                    {
                        case BankAccountType.Checking:
                            var _resultsChecking = await uow.DeleteCheckingAccountAsync(model as CheckingAccount);
                            if (_resultsChecking.Successful)
                            {
                                _deleted = true;
                                this.Dispose();
                            }
                            else
                            {
                                if (_resultsChecking.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsChecking.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsChecking.Message))
                                {
                                    WriteErrorCondition(_resultsChecking.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred deleting account record");
                                }
                            }
                            break;
                        case BankAccountType.Savings:
                            var _resultsSavings = await uow.DeleteSavingsAccountAsync(model as SavingsAccount);
                            if (_resultsSavings.Successful)
                            {
                                _deleted = true;
                                this.Dispose();
                            }
                            else
                            {
                                if (_resultsSavings.WorkException != null)
                                {
                                    WriteErrorCondition(_resultsSavings.WorkException);
                                }
                                else if (!string.IsNullOrEmpty(_resultsSavings.Message))
                                {
                                    WriteErrorCondition(_resultsSavings.Message);
                                }
                                else
                                {
                                    WriteErrorCondition("An unknown error has occurred deleting account record");
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                WriteErrorCondition("Account register must first be cleared before it can be deleted");
            }
            return _deleted;
        }

        public async Task<bool> DeleteRegisterItemAsync(AccountRegisterItemViewModel vm)
        {
            bool _deleted = false;

            switch (vm.ItemType)
            {
                case AccountRegisterItemViewModel.AccountItemType.Deposits:
                    switch (this.AccountType)
                    {
                        case BankAccountType.Checking:
                            _deleted = await (vm as CheckingDepositViewModel).DeleteAsync();
                            (vm as CheckingDepositViewModel).ItemUpdated -= OnRegisterUpdated;
                            break;
                        case BankAccountType.Savings:
                            _deleted = await (vm as SavingsDepositViewModel).DeleteAsync();
                            (vm as SavingsDepositViewModel).ItemUpdated -= OnRegisterUpdated;
                            break;
                    }
                    break;
                case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                    switch (this.AccountType)
                    {
                        case BankAccountType.Checking:
                            _deleted = await (vm as CheckingWithdrawalViewModel).DeleteAsync();
                            (vm as CheckingWithdrawalViewModel).ItemUpdated -= OnRegisterUpdated;
                            break;
                        case BankAccountType.Savings:
                            _deleted = await (vm as SavingsWithdrawalViewModel).DeleteAsync();
                            (vm as SavingsWithdrawalViewModel).ItemUpdated -= OnRegisterUpdated;
                            break;
                    }
                    break;
            }

            if (_deleted && this.AccountRegister.Contains(vm, new AccountRegisterItemViewModelComparer()))
            {
                this.AccountRegister.Remove(vm);
                GroupAccountItems();
            }

            return _deleted;
        }

        async void OnRegisterUpdated(object sender, BankingItemUpdatedEventArgs e)
        {
            // we need to retrieve the account and set the model property to the account.
            using (UnitOfWork uow = new UnitOfWork(this.dbFilePath))
            {
                switch (this.AccountType)
                {
                    case BankAccountType.Checking:
                        var _resultsChecking = Task.Run(() => uow.GetCheckingAccountAsync(model.id)).Result;
                        if (_resultsChecking.Successful)
                        {
                            this.CurrentBalance = _resultsChecking.Results.currentBalance;
                        }
                        break;
                    case BankAccountType.Savings:
                        var _resultsSavings = Task.Run(() => uow.GetSavingsAccountAsync(model.id)).Result;
                        if (_resultsSavings.Successful)
                        {
                            this.CurrentBalance = _resultsSavings.Results.currentBalance;
                        }
                        break;
                }

                await GroupAccountItemsAsync();
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDeposits)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalWithdrawals)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountRegisterGrouped)));
            }
        }

        public void Dispose()
        {
            foreach(AccountRegisterItemViewModel vm in this.AccountRegister)
            {
                switch (vm.ItemType)
                {
                    case AccountRegisterItemViewModel.AccountItemType.Deposits:
                        switch (this.AccountType)
                        {
                            case BankAccountType.Checking:
                                (vm as CheckingDepositViewModel).ItemUpdated -= OnRegisterUpdated;
                                break;
                            case BankAccountType.Savings:
                                (vm as SavingsDepositViewModel).ItemUpdated -= OnRegisterUpdated;
                                break;
                        }
                        break;
                    case AccountRegisterItemViewModel.AccountItemType.Withdrawals:
                        switch (this.AccountType)
                        {
                            case BankAccountType.Checking:
                                (vm as CheckingWithdrawalViewModel).ItemUpdated -= OnRegisterUpdated;
                                break;
                            case BankAccountType.Savings:
                                (vm as SavingsWithdrawalViewModel).ItemUpdated -= OnRegisterUpdated;
                                break;
                        }
                        break;
                }
            }
        }

        public override IChartDataPack GetChartData()
        {
            ChartDataPack chartPack = new ChartDataPack();

            ChartDataGroup allCategorizedGroup = new ChartDataGroup();
            allCategorizedGroup.ChartDisplayOrder = 0;
            allCategorizedGroup.ChartDisplayType = ChartType.Bar;

            List<List<AccountRegisterItemViewModel>> _registerVMsByCategory = new List<List<AccountRegisterItemViewModel>>();
            List<AccountRegisterItemViewModel> _allDepositTransactions = new List<AccountRegisterItemViewModel>();
            List<AccountRegisterItemViewModel> _allWithdrawalTransactions = new List<AccountRegisterItemViewModel>();

            decimal _depositSum = _allDepositTransactions.Sum(t => t.ItemAmount);
            decimal _withdrawalSum = _allWithdrawalTransactions.Sum(t => t.ItemAmount);

             _allDepositTransactions.AddRange(this.AccountRegister.ToList().Where(r => r.ItemType == AccountRegisterItemViewModel.AccountItemType.Deposits));
             _allWithdrawalTransactions.AddRange(this.AccountRegister.ToList().Where(r => r.ItemType == AccountRegisterItemViewModel.AccountItemType.Withdrawals));

            string _colorCode = string.Empty;
            List<AccountRegisterItemViewModel> _tempVMList = new List<AccountRegisterItemViewModel>();
            // Start with Deposits
            _registerVMsByCategory = new List<List<AccountRegisterItemViewModel>>();
            foreach (var _regVM in _allDepositTransactions.OrderBy(t => t.ItemDate))
            {
                _tempVMList.Add(_regVM);
            }
            foreach (var _regVM in _allWithdrawalTransactions.OrderBy(t => t.ItemDate))
            {
                _tempVMList.Add(_regVM);
            }

            //if (_tempVMList.Count > 0)
            //{
            //    _registerVMsByCategory.Add(_tempVMList);
            //}
            //foreach (var _list in _registerVMsByCategory)
            //{
            //    foreach (var _listItm in _list)
            //    {
            //        decimal _itmValue = _listItm.ItemAmount;
            //        ChartDataEntry _entry = new ChartDataEntry();
            //        _entry.FltValue = (float)(_itmValue);
            //        _entry.Label = "Item Value";
            //        _entry.ValueLabel = _itmValue.ToString("C");
            //        _entry.ColorCode = _listItm.ObjectColorCode;
            //        allCategorizedGroup.ChartDataItems.Add(_entry);
            //    }
            //    //decimal _catValue = _list.Sum(r => r.ItemAmount);
            //    //ChartDataEntry _entry = new ChartDataEntry();
            //    //_entry.FltValue = (float)_catValue;
            //    //_entry.Label = "Deposit Value";
            //    //_entry.ValueLabel = _catValue.ToString("C");
            //    //_entry.ColorCode = _list.First().ObjectColorCode;
            //    ////incomeCategorizedGroup.ChartDataItems.Add(_entry);
            //    //allCategorizedGroup.ChartDataItems.Add(_entry);
            //}

            foreach(var _itm in _tempVMList.OrderBy(vm => vm.ItemDate))
            {
                decimal _itmValue = _itm.ItemAmount;
                ChartDataEntry _entry = new ChartDataEntry();
                _entry.FltValue =  _itm.ItemType == AccountRegisterItemViewModel.AccountItemType.Deposits ? (float)(_itmValue)
                    : (float)(-1 * _itmValue);
                _entry.Label = "Item Value";
                _entry.ValueLabel = _itmValue.ToString("C");
                _entry.ColorCode = _itm.ObjectColorCode;
                allCategorizedGroup.ChartDataItems.Add(_entry);
            }

            // and Withdrawals
            //_registerVMsByCategory = new List<List<AccountRegisterItemViewModel>>();
            //foreach (var _regVM in _allWithdrawalTransactions.OrderBy(t => t.ItemDate))
            //{
            //    //if (_regVM.ObjectColorCode != _colorCode)
            //    //{
            //    //    if (_tempVMList.Count > 0)
            //    //    {
            //    //        _registerVMsByCategory.Add(_tempVMList);
            //    //    }
            //    //    _colorCode = _regVM.ObjectColorCode;
            //    //    _tempVMList = new List<AccountRegisterItemViewModel>();
            //    //}
            //    _tempVMList.Add(_regVM);
            //}
            //if (_tempVMList.Count > 0)
            //{
            //    _registerVMsByCategory.Add(_tempVMList);
            //}
            //foreach (var _list in _registerVMsByCategory)
            //{
            //    foreach(var _listItm in _list.OrderBy(vm => vm.ItemDate))
            //    {
            //        decimal _itmValue = _listItm.ItemAmount;
            //        ChartDataEntry _entry = new ChartDataEntry();
            //        _entry.FltValue = (float)(-1 * _itmValue);
            //        _entry.Label = "Item Value";
            //        _entry.ValueLabel = _itmValue.ToString("C");
            //        _entry.ColorCode = _listItm.ObjectColorCode;
            //        allCategorizedGroup.ChartDataItems.Add(_entry);
            //    }
            //    //decimal _catValue = _list.Sum(r => r.ItemAmount);
            //    //ChartDataEntry _entry = new ChartDataEntry();
            //    //_entry.FltValue = (float)(-1 * _catValue);
            //    //_entry.Label = "Category Value";
            //    //_entry.ValueLabel = 
            //    //_entry.ColorCode = _list.First().ObjectColorCode;
            //    ////spendingCategorizedGroup.ChartDataItems.Add(_entry);
            //    //allCategorizedGroup.ChartDataItems.Add(_entry);
            //}

            chartPack.Charts.Add(allCategorizedGroup);

            //List<List<AccountRegisterItemViewModel>> _registerVMsByCategory = new List<List<AccountRegisterItemViewModel>>();

            //string _colorCode = string.Empty;
            //List<AccountRegisterItemViewModel> _tempVMList = new List<AccountRegisterItemViewModel>();
            //foreach (var _regVM in this.AccountRegister.OrderBy(t => t.ObjectColorCode))
            //{
            //    if (_regVM.ObjectColorCode != _colorCode)
            //    {
            //        if (_tempVMList.Count > 0)
            //        {
            //            _registerVMsByCategory.Add(_tempVMList);
            //        }
            //        _colorCode = _regVM.ObjectColorCode;
            //        _tempVMList = new List<AccountRegisterItemViewModel>();
            //    }
            //    _tempVMList.Add(_regVM);
            //}
            //if (_tempVMList.Count > 0)
            //{
            //    _registerVMsByCategory.Add(_tempVMList);
            //}
            //foreach (var _list in _registerVMsByCategory)
            //{
            //    decimal _catValue = _list.Sum(r => r.ItemAmount);
            //    ChartDataEntry _entry = new ChartDataEntry();
            //    _entry.FltValue = (float)_catValue;
            //    _entry.Label = "Category Total";
            //    _entry.ColorCode = _list.First().ObjectColorCode;
            //    chartByCategoryGroup.ChartDataItems.Add(_entry);
            //}

            //chartPack.Charts.Add(chartByCategoryGroup);

            return chartPack;
        }
    }

    public class BankingItemUpdatedEventArgs : EventArgs
    {
        public BankAccountType AccountType { get; set; }
        public decimal TransactionAmount { get; set; }
    }

    public class BankAccountComparer : IEqualityComparer<BankAccountViewModel>
    {
        public bool Equals(BankAccountViewModel x, BankAccountViewModel y)
        {
            return x.AccountType == y.AccountType && x.AccountNumber == y.AccountNumber && x.BankName == y.BankName;
        }

        public int GetHashCode(BankAccountViewModel obj)
        {
            return obj.AccountNumber.GetHashCode() + obj.AccountType.GetHashCode() + obj.BankName.GetHashCode();
        }
    }
}
