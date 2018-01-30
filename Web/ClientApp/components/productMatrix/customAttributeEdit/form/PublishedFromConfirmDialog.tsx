import * as React from 'react'
import { Button } from 'reactstrap';
import Modal from '../../../common/Modal';

interface PublishedFromConfirmDialogProps {
    showDialog: boolean;
    closeDialog: () => void;
    confirmPublishedFrom: () => void;
}

const PublishedFromConfirmDialog = (props: PublishedFromConfirmDialogProps) =>
    <Modal isOpen={props.showDialog} dialogSize="md" handleDialogClose={props.closeDialog} footer={
        <>
        <Button color="primary" onClick={() => { props.confirmPublishedFrom(); props.closeDialog(); }}>Ano</Button>
        <Button color="default" onClick={props.closeDialog}>Ne</Button>
        </>
    }>
        <p>Datum publikování není nastaveno. Má se nová verze publikovat okamžitě (resp. od data platnosti)?</p>
    </Modal>

export default PublishedFromConfirmDialog