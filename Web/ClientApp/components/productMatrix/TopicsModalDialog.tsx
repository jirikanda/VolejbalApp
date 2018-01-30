import * as React from 'react';
import { Button } from 'reactstrap';
import Modal from '../common/Modal';
import Topics from '../../containers/topics/Topics';

interface TopicsModalDialogProps {
    selectedProductTypeId: number;
    isTopicsModalOpen: boolean;
    toggleTopicsModal: () => void;
    applyAndClose: () => void;
}

const TopicsModalDialog = (props: TopicsModalDialogProps) => {
    const {applyAndClose, isTopicsModalOpen, toggleTopicsModal, selectedProductTypeId} = props;

    return <Modal dialogSize="lg"
        footer={
            <Button color="primary" onClick={applyAndClose}>Upravit</Button>
        }
        isOpen={isTopicsModalOpen}
        title={"Témata"}
        handleDialogClose={toggleTopicsModal}>
        <Topics shouldPreserveSelection={false} selectedProductTypeId={selectedProductTypeId} />
    </Modal>
}

export default TopicsModalDialog