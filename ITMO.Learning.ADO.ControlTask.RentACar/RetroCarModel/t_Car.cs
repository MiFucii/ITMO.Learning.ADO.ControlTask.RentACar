//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ITMO.Learning.ADO.ControlTask.RentACar.RetroCarModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_Car
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public t_Car()
        {
            this.t_Сontract = new HashSet<t_Сontract>();
        }
    
        public string CarNumber { get; set; }
        public int IDInfoAuto { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string Cause { get; set; }
    
        public virtual t_InfoCar t_InfoCar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<t_Сontract> t_Сontract { get; set; }
    }
}
