using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Nastenka;

public interface INastenkaFacade
{
    Task<VzkazListDto> GetVzkazy();

    Task VlozVzkaz(VzkazInputDto vzkaz);
}
