using System;
using System.Linq;
using System.Web.Mvc;
using purrfect_blog_starter_1757957984.Models;

namespace purrfect_blog_starter_1757957984.Controllers
{
    public class PostsController : Controller
    {
        [HttpGet]
        [Authorize]
        public ActionResult CreatePost()
        {
            return View(new PostCreateViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(PostCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            using (var db = new ApplicationDbContext())
            {
                var post = new Post
                {
                    Title = model.Title?.Trim(),
                    Content = model.Content?.Trim(),
                    Category = model.Category?.Trim(),
                    AuthorUsername = username,
                    CreatedAt = DateTime.UtcNow
                };
                db.Posts.Add(post);
                db.SaveChanges();
            }

            TempData["Message"] = "Post created successfully.";
            return RedirectToAction("Dashboard", "Account");
        }

        // sort: recent | oldest | mostUpvoted | mostDownvoted
        [HttpGet]
        public ActionResult Index(string sort = "recent")
        {
            using (var db = new ApplicationDbContext())
            {
                var currentUser = User?.Identity?.Name;

                var query =
                    from p in db.Posts
                    join v in db.Votes on p.Id equals v.PostId into pv
                    select new PostListItemViewModel
                    {
                        Post = p,
                        Score = pv.Select(x => (int?)x.Value).Sum() ?? 0,
                        CurrentUserVote = currentUser == null
                            ? 0
                            : pv.Where(x => x.VoterUsername == currentUser).Select(x => x.Value).FirstOrDefault()
                    };

                switch ((sort ?? "recent").ToLowerInvariant())
                {
                    case "oldest":
                        query = query.OrderBy(x => x.Post.CreatedAt);
                        break;
                    case "mostupvoted":
                        query = query.OrderByDescending(x => x.Score);
                        break;
                    case "mostdownvoted":
                        query = query.OrderBy(x => x.Score);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.Post.CreatedAt);
                        sort = "recent";
                        break;
                }

                var items = query.ToList();
                ViewBag.Sort = sort;
                return View(items);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Upvote(int id) => VoteInternal(id, +1);

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Downvote(int id) => VoteInternal(id, -1);

        private ActionResult VoteInternal(int postId, int value)
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            using (var db = new ApplicationDbContext())
            {
                var postExists = db.Posts.Any(p => p.Id == postId);
                if (!postExists) return HttpNotFound();

                var vote = db.Votes.FirstOrDefault(v => v.PostId == postId && v.VoterUsername == username);
                if (vote == null)
                {
                    // First time voting on this post
                    db.Votes.Add(new Vote
                    {
                        PostId = postId,
                        VoterUsername = username,
                        Value = value,
                        CreatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                    //TempData["Message"] = "Vote recorded.";
                }
                else if (vote.Value == value)
                {
                    // Clicking the same button again -> undo (remove vote)
                    db.Votes.Remove(vote);
                    db.SaveChanges();
                    //TempData["Message"] = "Vote removed.";
                }
                else
                {
                    // Switching from up->down or down->up
                    vote.Value = value;
                    db.SaveChanges();
                    //TempData["Message"] = "Vote updated.";
                }
            }

            var referer = Request?.UrlReferrer?.ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }
    }
}