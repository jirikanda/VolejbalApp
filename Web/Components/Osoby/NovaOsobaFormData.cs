using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Osoby;

public class NovaOsobaFormData
{
    [Required(ErrorMessage = "Zadej příjmení.")]
    [StringLength(50, ErrorMessage = "Maximální délka příjmení je 50 znaků.")]
    public string Prijmeni { get; set; }

    [Required(ErrorMessage = "Zadej jméno.")]
    [StringLength(50, ErrorMessage = "Maximální délka jména je 50 znaků.")]
    public string Jmeno { get; set; }

    [Required(ErrorMessage = "Zadej email.")]
    [StringLength(50, ErrorMessage = "Maximální délka emailu je 50 znaků.")]
    public string Email { get; set; }
}
