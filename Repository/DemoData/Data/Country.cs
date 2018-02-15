using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;


namespace DemoData.Data
{
 


     [DataContract]
     [Table("Country")]
    public partial class Country
    {
 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Country()
        {
            Mentor = new HashSet<Mentor>();
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public string Code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mentor> Mentor { get; set; }
    }
}
