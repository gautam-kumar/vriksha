ROOTNAME = paper 


all: paper 

paper:
	xelatex $(ROOTNAME)
	bibtex $(ROOTNAME)
	xelatex $(ROOTNAME)
	xelatex $(ROOTNAME)


clean:
	rm -f *.out *.dvi *.ps *.pdf *.aux *.bbl *.blg *.log $(DRAFT_SETTINGS)

