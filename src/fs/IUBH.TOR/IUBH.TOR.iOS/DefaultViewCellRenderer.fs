namespace IUBH.TOR.iOS

open UIKit
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

/// The default behavior of XF will leave us with a selected
/// cell that can't be unselected. This custom renderer fixes that.
type DefaultViewCellRenderer() =
    inherit ViewCellRenderer()

    override this.GetCell(item, reusableCell, tableView) =
      let cell = base.GetCell(item, reusableCell, tableView)
      cell.SelectionStyle <- UITableViewCellSelectionStyle.None
      cell

[<assembly: ExportRendererAttribute (typeof<ViewCell>, typeof<DefaultViewCellRenderer>)>] do ()
