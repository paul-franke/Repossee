using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System;
namespace DemoData.Data
{

    [DataContract]
    [Table("InternShip")]
    public partial class InternShip
    {

 
        [DataMember]
        [Required]
        public int Id { get; set; }

        [DataMember]
        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        [DataMember]
        [Required]
        public Boolean Accredited { get; set; }

        [DataMember]
        [ForeignKey("Student")]
        public int? StudentId { get; set; }

        [DataMember]
        public DateTime? StartDate { get; set; }

        public virtual Student Student { get; set; }

    }

}

