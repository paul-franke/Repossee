using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace DemoData.Data
{

    [DataContract]
    [Table("Mentor")]
    public partial class Mentor
    {

        [DataMember]
        [Required]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        [Required]
        public string Address { get; set; }

        [DataMember]
        [Required]
        public string Email { get; set; }

        [DataMember]
        [Required]
        [ForeignKey("Country")]

        public int Country_Id { get; set; }

 
        public virtual Country Country { get; set; }
    }

}
