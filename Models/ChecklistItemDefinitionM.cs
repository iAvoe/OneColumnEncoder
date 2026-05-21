using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Models
{
    public record ChecklistItemDefinitionM(string Text, StatusType InitialStatus = StatusType.Waiting);
}