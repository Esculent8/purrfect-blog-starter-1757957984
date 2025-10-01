using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace purrfect_blog_starter_1757957984.Models
{
    public class Vote
    {
        public int Id { get; set; }

        [Index("IX_Post_Voter", 1, IsUnique = true)]
        public int PostId { get; set; }

        [Required, StringLength(100)]
        [Index("IX_Post_Voter", 2, IsUnique = true)]
        public string VoterUsername { get; set; }

        // +1 = upvote, -1 = downvote
        [Range(-1, 1)]
        public int Value { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}