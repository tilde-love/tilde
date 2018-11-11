// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Tilde.Host.Hubs.Module
{
    public interface IModuleClient
    {
        Task OnValueChanged(Uri uri, string connectionId, object value);
    }
}