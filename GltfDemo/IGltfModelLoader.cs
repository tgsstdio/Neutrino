using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GltfDemo
{
    interface IGltfModelLoader
    {
        void Load(IEffect effect, string modelFilePath);
    }
}
