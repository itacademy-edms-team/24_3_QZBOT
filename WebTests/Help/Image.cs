using WebTests.Models;

namespace WebTests.Help
{
    public static class Image
    {
        public async static Task<string> UserAvatar(IFormFile avatar, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", url.TrimStart('/'));

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
            var filePath = Path.Combine("wwwroot", "avatars", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            url = $"/avatars/{fileName}";

            return url;
        }

        public async static Task<string> TestCover(IFormFile cover, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var oldPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    url.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(cover.FileName);

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/covers",
                fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await cover.CopyToAsync(stream);
            }

            var newCover = "/covers/" + fileName;
            return newCover;
        }
    }
}
