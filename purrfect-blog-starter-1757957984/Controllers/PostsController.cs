using System;
using System.Linq;
using System.Web.Mvc;
using purrfect_blog_starter_1757957984.Models;

namespace purrfect_blog_starter_1757957984.Controllers
{
    // Handles creating posts, listing/sorting posts, voting, and viewing details
    public class PostsController : Controller
    {
        // Render the Create Post form (only for authenticated users)
        [HttpGet]
        [Authorize]
        public ActionResult CreatePost()
        {
            // Provide an empty view model to drive validation/UI
            return View(new PostCreateViewModel());
        }

        // Create a new post (POST with anti-forgery and model validation)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(PostCreateViewModel model)
        {
            // Server-side validation check
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check the current username; reject if missing
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            // Trim inputs, store timestamp
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

            // Flash message then redirect user to their dashboard
            TempData["Message"] = "Post created successfully.";
            return RedirectToAction("Dashboard", "Account");
        }

        // Global posts list with sorting: recent | oldest | mostUpvoted | mostDownvoted
        [HttpGet]
        public ActionResult Index(string sort = "recent")
        {
            using (var db = new ApplicationDbContext())
            {
                var currentUser = User?.Identity?.Name;

                // Build a projection that includes post + aggregate score
                // and the current user's vote (if authenticated)
                var query =
                    from p in db.Posts
                    join v in db.Votes on p.Id equals v.PostId into pv
                    select new PostListItemViewModel
                    {
                        Post = p,
                        // Sum votes (+1/-1) with null-safe default to 0
                        Score = pv.Select(x => (int?)x.Value).Sum() ?? 0,
                        // Resolve current user's vote value, or 0 if not signed in
                        CurrentUserVote = currentUser == null
                            ? 0
                            : pv.Where(x => x.VoterUsername == currentUser).Select(x => x.Value).FirstOrDefault()
                    };

                // Apply sort (default to "recent" if input is unrecognized)
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

        // Vote endpoints: toggle behavior handled inside VoteInternal.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Upvote(int id) => VoteInternal(id, +1);

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Downvote(int id) => VoteInternal(id, -1);

        // Toggle voting logic:
        // - First vote creates a record.
        // - Re-clicking same vote removes it
        // - Clicking opposite vote flips the value
        private ActionResult VoteInternal(int postId, int value)
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            using (var db = new ApplicationDbContext())
            {
                // Validate target post exists
                var postExists = db.Posts.Any(p => p.Id == postId);
                if (!postExists) return HttpNotFound();

                // Find existing vote by this user for this post
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
                }
                else if (vote.Value == value)
                {
                    // Clicking the same button again -> undo (remove vote)
                    db.Votes.Remove(vote);
                    db.SaveChanges();
                }
                else
                {
                    // Switching from up->down or down->up
                    vote.Value = value;
                    db.SaveChanges();
                }
            }

            // Redirect back to the originating page if available; else back to index
            var referer = Request?.UrlReferrer?.ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }

        // Post details view (404 if not found)
        [HttpGet]
        public ActionResult Details(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var post = db.Posts.FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    // Return a friendly page when the post does not exist
                    Response.StatusCode = 404;
                    Response.TrySkipIisCustomErrors = true;
                    return View("PostDeleted");
                }
                return View(post);
            }
        }

        // Render the Edit Post form: /EditPost/{id}
        [HttpGet]
        [Authorize]
        public ActionResult EditPost(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var post = db.Posts.FirstOrDefault(p => p.Id == id);
                if (post == null) return HttpNotFound();

                var username = User?.Identity?.Name;
                if (!string.Equals(post.AuthorUsername, username, StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpUnauthorizedResult();
                }

                var model = new PostCreateViewModel
                {
                    Title = post.Title,
                    Content = post.Content,
                    Category = post.Category
                };

                ViewBag.PostId = post.Id;
                ViewBag.PostTitle = post.Title;
                return View(model);
            }
        }

        // Update the post
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int id, PostCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostId = id;
                return View(model);
            }

            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            using (var db = new ApplicationDbContext())
            {
                var post = db.Posts.FirstOrDefault(p => p.Id == id);
                if (post == null) return HttpNotFound();

                if (!string.Equals(post.AuthorUsername, username, StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpUnauthorizedResult();
                }

                post.Title = model.Title?.Trim();
                post.Content = model.Content?.Trim();
                post.Category = model.Category?.Trim();
                db.SaveChanges();

                TempData["Message"] = "Post updated successfully.";
                return RedirectToAction("Details", new { id = post.Id });
            }
        }

        // Delete a post (author only)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var username = User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                return new HttpUnauthorizedResult();
            }

            using (var db = new ApplicationDbContext())
            {
                var post = db.Posts.FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    // Treat as already deleted
                    TempData["Message"] = "Post not found or already deleted.";
                    return RedirectToAction("Index");
                }

                if (!string.Equals(post.AuthorUsername, username, StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpUnauthorizedResult();
                }

                try
                {
                    // Remove related votes via SQL to avoid EF concurrency on child deletes
                    db.Database.ExecuteSqlCommand("DELETE FROM [Votes] WHERE [PostId] = @p0", id);

                    // Now remove the post
                    db.Posts.Remove(post);

                    db.SaveChanges();

                    TempData["Message"] = "Post deleted.";
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Core.OptimisticConcurrencyException)
                {
                    // If another request already deleted the post (or cascade already removed rows),
                    // just show a friendly message and continue.
                    TempData["Message"] = "Post was already deleted.";
                    return RedirectToAction("Index");
                }
            }
        }
    }
}