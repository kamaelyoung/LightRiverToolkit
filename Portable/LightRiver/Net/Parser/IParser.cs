﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net
{
    public interface IParser<TSource>
    {
        object Parse(TSource source);
    }
}
