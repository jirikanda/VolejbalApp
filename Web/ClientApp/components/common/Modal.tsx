import { Modal as ReactstrapModal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import * as React from 'react';

interface ModalProps extends React.HTMLProps<HTMLDivElement> {
    handleDialogClose: () => void;
    isOpen: boolean;
    dialogSize?: string;
    title?: string;
    dialogClassName?: string
    footer?: JSX.Element
}

const Modal = (props: ModalProps) => {
    const { isOpen, handleDialogClose, title, children, dialogClassName, dialogSize, footer } = props;

    return <ReactstrapModal isOpen={isOpen} backdrop={true} toggle={handleDialogClose} className={dialogClassName} size={dialogSize}>
        <ModalHeader>
            {title}{" "}
            <button type="button" className="close pointer" aria-label="Close" onClick={handleDialogClose} title={"Close"}>
                <i className="fa fa-times" aria-hidden="true"></i>
            </button>
        </ModalHeader>
        <ModalBody>
            { children }
        </ModalBody>
        { footer &&
            <ModalFooter>
                {footer}
            </ModalFooter>
        }
    </ReactstrapModal>
}

export default Modal;