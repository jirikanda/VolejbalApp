import * as React from 'react';
import { Button } from 'reactstrap';
import Modal from '../../../common/Modal';

interface CurrentCopyToDraftConfirmDialogProps {
    showDialog: boolean;
    closeDialog: () => void;
    copyCurrentToDraft: () => void;
}

const CurrentCopyToDraftConfirmDialog = (props: CurrentCopyToDraftConfirmDialogProps) =>
    <Modal isOpen={props.showDialog} dialogSize="md" handleDialogClose={props.closeDialog} footer={
        <>
        <Button color="primary" onClick={() => { props.copyCurrentToDraft(); props.closeDialog(); }}>Ano</Button>
        <Button color="default" onClick={props.closeDialog}>Ne</Button>
        </>
    }>
        <p>Opravdu chcete přepsat aktuální hodnoty do konceptu?</p>
    </Modal>

export default CurrentCopyToDraftConfirmDialog