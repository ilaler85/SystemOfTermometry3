using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfThermometry3.DeviceWorking;

enum PortExceptionEnum
{
    PortNotExist,
    PortIsUsing,
    PortWrongName,
    PortCloseError,
    PortUnknowingError,

    PortOpenSuccessfully,
    PortCloseSuccessfully
}
