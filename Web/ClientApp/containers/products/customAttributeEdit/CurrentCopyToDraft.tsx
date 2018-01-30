import * as React from 'react';
import CurrentCopyToDraftButton from '../../../components/productMatrix/customAttributeEdit/form/CurrentCopyToDraftButton';
import { connect } from 'react-redux';
import { getFormValues, RegisteredField } from 'redux-form';
import CurrentCopyToDraftConfirmDialog from '../../../components/productMatrix/customAttributeEdit/form/CurrentCopyToDraftConfirmDialog';
import { FormName as CurrentFormName } from '../../../components/productMatrix/customAttributeEdit/CurrentCustomAttributeValueForm';
import { FormName as DraftFormName } from '../../../components/productMatrix/customAttributeEdit/DraftCustomAttributeValueForm';

interface FormInstance {
    registeredFields?: { [name: string]: RegisteredField };
}

interface CurrentCopyToDraftProps { 
    currentFormInstance?: FormInstance;
    draftFormInstance?: FormInstance;
    formValues?: Object;
    onCopyCurrentToDraft: (filedName: string, fieldValue: string) => void;
}

interface CurrentCopyToDrafState {
    showDialog: boolean;
}

const CurrentRegisteredFieldRegExp = /^current\.(.)+$/i;
const DraftRegisteredFieldRegExp = /^draft\.(.)+$/i;

class CurrentCopyToDraft extends React.Component<CurrentCopyToDraftProps, CurrentCopyToDrafState> {
    constructor(props: CurrentCopyToDraftProps) {
        super(props);

        this.state = {
            showDialog: false
        }
    }
    openModal = () => this.setState({ showDialog: true })
    closeModal = () => this.setState({ showDialog: false })

    copyCurrentToDraft = () => {
        if (this.props.currentFormInstance && this.props.draftFormInstance) {
            const currentRegistredFields = Object.keys(this.props.currentFormInstance.registeredFields).map(key => this.props.currentFormInstance.registeredFields[key]);
            const draftRegistredFields = Object.keys(this.props.draftFormInstance.registeredFields).map(key => this.props.draftFormInstance.registeredFields[key]);

            const currentFields = currentRegistredFields.filter(registredField => CurrentRegisteredFieldRegExp.test(registredField.name))
            const draftFields = draftRegistredFields.filter(registredField => DraftRegisteredFieldRegExp.test(registredField.name))

            const valuesKeys = Object.keys(this.props.formValues);

            currentFields.forEach(currentField => {
                const currentFieldSuffix = currentField.name.substring("current.".length);
                const draftFieldMatch = draftFields.find(draftField => draftField.name.endsWith(currentFieldSuffix));

                const currentFieldNameParts = currentField.name.split('.');

                if (draftFieldMatch && valuesKeys.find(valueKey => currentFieldNameParts[0] === valueKey)) {
                    this.props.onCopyCurrentToDraft(draftFieldMatch.name, this.props.formValues[currentFieldNameParts[0]][currentFieldNameParts[1]]); // current field value to draft field value
                }
            })
        }
    }

    render() {
        return <>
            <CurrentCopyToDraftButton openModal={this.openModal} />
            <CurrentCopyToDraftConfirmDialog showDialog={this.state.showDialog} closeDialog={this.closeModal} copyCurrentToDraft={this.copyCurrentToDraft} />
        </>
    }
}

export default connect((state, props: CurrentCopyToDraftProps) => {
    return {
        currentFormInstance: state.form[CurrentFormName],
        draftFormInstance: state.form[DraftFormName],
        formValues: getFormValues(CurrentFormName)(state)
    };
}, null)(CurrentCopyToDraft)