import { Button, InputGroup, InputGroupButton } from 'reactstrap';
import Modal from '../common/Modal';
import * as React from 'react';
import * as TopicStore from '../../store/Topics';
import { ProductType } from '../../apimodels/SegmentsStore';
import * as toastr from 'toastr';

interface SelectedTopicsSaveButtonProps {
    saveTopicsSelection: (selectedProductTypeId: number, selectionName: string) => void;
    selectedProductTypeId: number;
}

interface SelectedTopicsSaveButtonState {
    isOpen: boolean;
}

class SelectedTopicsSaveButton extends React.Component<SelectedTopicsSaveButtonProps, SelectedTopicsSaveButtonState> {
    private inputNamaRef: HTMLInputElement;

    constructor(props) {
        super(props)

        this.state = {
            isOpen: false
        }
    }

    handleSaveTopicsSelection = () => {
        this.props.saveTopicsSelection(this.props.selectedProductTypeId, this.inputNamaRef.value);
        this.handleDialogClose();
    }

    handleDialogClose = () => {
        this.setState({
            isOpen: false
        });
    }

    handleOnClickShowButton = () => {
        this.setState({
            isOpen: true
        });
    }

    render() {
        const { isOpen } = this.state;

        return <><Button key="button" color="primary" outline onClick={this.handleOnClickShowButton}>Uložit výběr témat</Button>
            <Modal key="modal" isOpen={isOpen} title={"Uložit nastavení výběru témat"} handleDialogClose={this.handleDialogClose}>
                <InputGroup>
                    <input type="text" ref={ref => this.inputNamaRef = ref} className="form-control" placeholder="Zadejte název výběru témat" />
                    <InputGroupButton>
                        <Button className="pull-right" color="primary" onClick={this.handleSaveTopicsSelection}>Uložit</Button>
                    </InputGroupButton>
                </InputGroup>
            </Modal></>
    }
}

export default SelectedTopicsSaveButton;