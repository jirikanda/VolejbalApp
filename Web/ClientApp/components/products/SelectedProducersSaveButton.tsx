import {Button} from 'reactstrap';
import * as React from 'react';
import * as toastr from 'toastr';

interface SelectedProducersSaveButtonProps {
    saveProducersSelection: (selectedProductTypeId: number) => void;
    selectedProductTypeId: number;
}

const SelectedProducersSaveButton = ({selectedProductTypeId, saveProducersSelection}: SelectedProducersSaveButtonProps) =>
{
    const handleButtonClick = () => {
        saveProducersSelection(selectedProductTypeId);
    }

    return <Button outline color="primary" onClick={handleButtonClick} >
        Zapamatovat producenty
    </Button>
}

export default SelectedProducersSaveButton;