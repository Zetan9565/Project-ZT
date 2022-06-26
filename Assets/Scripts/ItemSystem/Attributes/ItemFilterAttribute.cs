using System;

namespace ZetanStudio.ItemSystem
{
    public class ItemFilterAttribute : Attribute
    {
        public readonly object[] filters;

        public ItemFilterAttribute(object filter)
        {
            filters = new object[] { filter };
        }
        public ItemFilterAttribute(params object[] filters)
        {
            this.filters = filters;
        }
        public ItemFilterAttribute(params Type[] modules)
        {
            filters = modules;
        }

        public bool DoFilter(Item item)
        {
            foreach (var filter in filters)
            {
                if (!CheckFilter(filter, item))
                    return false;
            }
            return true;
        }

        private bool CheckFilter(object filter, Item item)
        {
            if (!item) return false;
            if (filter is string f)
            {
                f = f.ToLower();
                if (f == "clear") return true;
                bool revers = f.StartsWith("not ");
                f = f.Replace("not ", "");
                string value = f.Split(':')[^1];
                if (f.StartsWith("n:") || f.StartsWith("name:"))
                    if (revers) return !item.Name.Contains(value);
                    else return item.Name.Contains(value);
                else if (f.StartsWith("i:") || f.StartsWith("id:"))
                    if (revers) return !item.ID.Contains(value);
                    else return item.ID.Contains(value);
                else if (f.StartsWith("t:") || f.StartsWith("type:"))
                    if (int.TryParse(value, out var index))
                        if (revers) return ItemTypeEnum.IndexOf(item.Type) != index;
                        else return ItemTypeEnum.IndexOf(item.Type) == index;
                    else if (revers) return item.Quality.Name != value;
                    else return item.Quality.Name == value;
                else if (f.StartsWith("q:") || f.StartsWith("quality:"))
                    if (int.TryParse(value, out var index))
                        if (revers) return ItemQualityEnum.IndexOf(item.Quality) != index;
                        else return ItemQualityEnum.IndexOf(item.Quality) == index;
                    else if (revers) return item.Quality.Name != value;
                    else return item.Quality.Name == value;
                else if (f.StartsWith("d:") || f.StartsWith("des:") || f.StartsWith("desc:") || f.StartsWith("description:"))
                    if (revers) return !item.Description.Contains(value);
                    else return item.Description.Contains(value);
                else if (f.StartsWith("m:") || f.StartsWith("module:"))
                    foreach (var module in item.Modules)
                    {
                        if (revers && module.GetType().Name != value || !revers && module.GetType().Name == value)
                            return true;
                    }
                return false;
            }
            else if (filter is Type type)
                foreach (var module in item.Modules)
                {
                    if (module.GetType() == type)
                        return true;
                }
            return false;
        }
    }
}