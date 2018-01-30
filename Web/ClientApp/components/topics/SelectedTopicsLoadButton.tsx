import { Button, Table } from 'reactstrap';
import Modal from '../common/Modal';
import * as React from 'react';
import * as TopicStore from '../../store/Topics';
import { ProductType } from '../../apimodels/SegmentsStore';

import * as toastr from 'toastr';
import SavedTopicsSelectionsItem from './SavedTopicsSelectionsItem';

interface SelectedTopicsLoadButtonProps {
    loadTopicsSelections: (selectedProductTypeId: number) => void;
    loadTopicsFromSelection: (selectedProductTypeId: number, selectionId: number) => void;
    removeTopicsSelection: (selectedProductTypeId: number, selectionId: number) => void;
    savedTopicsSelections: TopicStore.SavedTopicsSelection[];
    selectedProductTypeId: number;
}

interface SelectedTopicsLoadButtonState {
    isOpen: boolean;
}

class SelectedTopicsLoadButton extends React.Component<SelectedTopicsLoadButtonProps, SelectedTopicsLoadButtonState> {
    constructor(props) {
        super(props)

        this.state = {
            isOpen: false
        }
    }

    handleDialogClose = () => {
        this.setState({
            isOpen: false
        });
    }

    handleOnClickShowButton = () => {
        const {loadTopicsSelections, selectedProductTypeId} = this.props;

        this.setState({
            isOpen: true
        });

        loadTopicsSelections(selectedProductTypeId);
    }

    render() {
        const {isOpen} = this.state;
        const {savedTopicsSelections} = this.props;
        const selections = savedTopicsSelections.map(selection =>
            <SavedTopicsSelectionsItem {...this.props}
                key={selection.topicSelectionId}
                id={selection.topicSelectionId}
                handleDialogClose={this.handleDialogClose}
                selectionName={selection.topicSelectionName} />
        );

        return <span>
            <Button color="primary" outline onClick={this.handleOnClickShowButton}>
                Moje témata
            </Button>
            <Modal isOpen={isOpen} title={"Moje témata"} handleDialogClose={this.handleDialogClose}>
                <Table hover striped className="topic-table">
                    <tbody>
                        {selections.length > 0 ? selections : <tr><td>Zatím nemáte uložená žádná témata.</td></tr>}
                    </tbody>
                </Table>
            </Modal>
        </span>
    }
}

export default SelectedTopicsLoadButton;