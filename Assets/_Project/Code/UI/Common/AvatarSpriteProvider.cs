using System.Collections.Generic;
using EmployeeContacts.Services;
using UnityEngine;

namespace EmployeeContacts.UI.Common
{
    public sealed class AvatarSpriteProvider
    {
        private readonly AvatarService _avatarService;
        private readonly Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>();

        public AvatarSpriteProvider(AvatarService avatarService)
        {
            _avatarService = avatarService;
        }

        public Sprite GetSprite(int employeeId)
        {
            if (!_avatarService.TryGetImageId(employeeId, out var imageId))
            {
                return CreatePlaceholderSprite();
            }

            if (_sprites.TryGetValue(imageId, out var cachedSprite))
            {
                return cachedSprite;
            }

            var texture = _avatarService.LoadAvatarTexture(employeeId);
            var sprite = CreateSprite(texture);
            _sprites[imageId] = sprite;
            return sprite;
        }

        public void Dispose()
        {
            foreach (var sprite in _sprites.Values)
            {
                if (sprite == null)
                {
                    continue;
                }

                var texture = sprite.texture;
                Object.Destroy(sprite);
                if (texture != null && texture != _avatarService.PlaceholderTexture)
                {
                    Object.Destroy(texture);
                }
            }

            _sprites.Clear();
        }

        private Sprite CreatePlaceholderSprite()
        {
            return CreateSprite(_avatarService.PlaceholderTexture);
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
