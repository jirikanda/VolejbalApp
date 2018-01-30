import {Button} from 'reactstrap';
import * as React from 'react';
import * as toastr from 'toastr';

interface SelectedProducersLoadButtonProps {
    loadProducersSelection: (selectedProductTypeId: number) => void;
    selectedProductTypeId: number;
}

const SelectedProducersLoadButton = ({selectedProductTypeId, loadProducersSelection}: SelectedProducersLoadButtonProps) =>
{
    const handleButtonClick = () => {
        loadProducersSelection(selectedProductTypeId);
    }

    return <Button outline color="primary" onClick={handleButtonClick} >
        Moji producenti
    </Button>
}

export default SelectedProducersLoadButton;