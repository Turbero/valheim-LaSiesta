namespace LaSiesta.Tweaks
{
    public class CursorManager
    {
        public static void showCursor()
        {   
            Menu.instance.Show();
            Menu.instance.transform.Find("MenuRoot/Menu/MenuEntries").gameObject.SetActive(false);
            Menu.instance.transform.Find("MenuRoot/Menu/ornament").gameObject.SetActive(false);
        }

        public static void hideCursor()
        {
            Menu.instance.transform.Find("MenuRoot/Menu/MenuEntries").gameObject.SetActive(true);
            Menu.instance.transform.Find("MenuRoot/Menu/ornament").gameObject.SetActive(true);
            Menu.instance.Hide();
        }
    }
}