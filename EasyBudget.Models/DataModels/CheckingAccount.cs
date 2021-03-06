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
using SQLite;

namespace EasyBudget.Models.DataModels
{
    [Table("CheckingAccount")]
    public class CheckingAccount : BankAccount
    {
        
        [Ignore]
        public ICollection<CheckingDeposit> deposits { get; set; }

        [Ignore]
        public ICollection<CheckingWithdrawal> withdrawals { get; set; }

        public CheckingAccount()
        {
        }
    }
}
