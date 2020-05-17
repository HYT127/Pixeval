﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Pixeval.Data.ViewModel;
using SQLite;

namespace Pixeval.Core
{
    public class BrowsingHistoryAccessor : ITimelineService, IDisposable
    {
        public static BrowsingHistoryAccessor GlobalLifeTimeScope;
        private readonly ObservableCollection<BrowsingHistory> delegation;
        private readonly string path;
        private readonly SQLiteConnection sqLiteConnection;
        private readonly int stackLimit;
        private bool writable;

        public BrowsingHistoryAccessor(int stackLimit, string path)
        {
            this.stackLimit = stackLimit;
            this.path = path;
            sqLiteConnection = new SQLiteConnection(path, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            sqLiteConnection.CreateTable<BrowsingHistory>();
            delegation = new ObservableCollection<BrowsingHistory>(sqLiteConnection.Table<BrowsingHistory>());
        }

        public void Dispose()
        {
            sqLiteConnection?.Dispose();
        }

        public bool VerifyRationality(BrowsingHistory browsingHistory)
        {
            if (delegation.Any())
            {
                var prev = delegation[0];
                if (prev.Type == browsingHistory.Type && prev.BrowseObjectId == browsingHistory.BrowseObjectId)
                    return false;
            }

            return true;
        }

        public void Insert(BrowsingHistory browsingHistory)
        {
            if (delegation.Count >= stackLimit) delegation.Remove(delegation.Last());

            delegation.Insert(0, browsingHistory);
        }

        public void EmergencyRewrite()
        {
            if (File.Exists(path)) throw new InvalidOperationException();
            using var sql = new SQLiteConnection(path);
            sql.CreateTable<BrowsingHistory>();
            sql.InsertAll(Get());
        }

        public IEnumerable<BrowsingHistory> Get()
        {
            return delegation;
        }

        public void Rewrite()
        {
            if (!writable) throw new InvalidOperationException();
            sqLiteConnection.DropTable<BrowsingHistory>();
            sqLiteConnection.CreateTable<BrowsingHistory>();
            sqLiteConnection.InsertAll(delegation);
        }

        public void DropDb()
        {
            if (File.Exists(path)) File.Delete(path);
        }

        public void SetWritable()
        {
            writable = true;
        }
    }
}