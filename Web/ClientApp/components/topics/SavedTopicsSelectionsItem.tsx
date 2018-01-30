import * as React from "react";
import * as toastr from "toastr";
import { Table } from "reactstrap";

interface SavedTopicsSelectionsItemProps {
  id: number;
  selectedProductTypeId: number;
  selectionName: string;
  handleDialogClose: () => void;
  loadTopicsFromSelection: (
    selectedProductTypeId: number,
    selectionId: number
  ) => void;
  removeTopicsSelection: (
    selectedProductTypeId: number,
    selectionId: number
  ) => void;
}

const SavedTopicsSelectionsItem = ({
  id,
  selectedProductTypeId,
  selectionName,
  loadTopicsFromSelection,
  removeTopicsSelection,
  handleDialogClose
}: SavedTopicsSelectionsItemProps) => {
  const handleItemClick = () => {
    loadTopicsFromSelection(selectedProductTypeId, id);
    handleDialogClose();
  };

  const handleRemoveButtonClick = (e: React.SyntheticEvent<EventTarget>) => {
    e.stopPropagation();
    removeTopicsSelection(selectedProductTypeId, id);
  };

  return (
    <tr onClick={handleItemClick}>
      <td>
        {" "}
        {selectionName}
        <i
          onClick={handleRemoveButtonClick}
          className="fa fa-trash-o pull-right pointer"
          aria-hidden="true"
        />
      </td>
    </tr>
  );
};

export default SavedTopicsSelectionsItem;
