namespace purrfect_blog_starter_1757957984.Models
{
    public class PostListItemViewModel
    {
        public Post Post { get; set; }
        public int Score { get; set; }            // Sum of votes
        public int CurrentUserVote { get; set; }  // -1, 0, +1
    }
}