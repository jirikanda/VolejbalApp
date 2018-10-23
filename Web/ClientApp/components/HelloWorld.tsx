import * as React from 'react'
import { withStyles } from '@material-ui/core/styles';
import Paper from '@material-ui/core/Paper';
import Typography from '@material-ui/core/Typography';
import { withRouter } from 'react-router';
import { compose } from 'redux';
import { NavLink } from 'react-router-dom';

const styles = theme => ({
    root: {
        ...theme.mixins.gutters(),
        paddingTop: theme.spacing.unit * 2,
        paddingBottom: theme.spacing.unit * 2,
    },
});

const HelloWorld = (props) => {
    const { classes } = props;

    return <>        
        <Paper className={classes.root} elevation={1}>
            <Typography variant="headline" component="h3">
                Hello world
            </Typography>
            <NavLink to='/'>Home</NavLink>
        </Paper>
    </>;
}

export default compose(withRouter, withStyles(styles))(HelloWorld)