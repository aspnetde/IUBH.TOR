using IUBH.TOR.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ViewCell), typeof(DefaultViewCellRenderer))]

namespace IUBH.TOR.iOS.CustomRenderers
{
    /// <summary>
    /// Makes sure cells in lists do not appear to be selected. This solves
    /// an ugly glitch in the current Xamarin.Forms implementation.
    /// </summary>
    public class DefaultViewCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(
            Cell item,
            UITableViewCell reusableCell,
            UITableView tv
        )
        {
            var cell = base.GetCell(item, reusableCell, tv);

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }
    }
}
